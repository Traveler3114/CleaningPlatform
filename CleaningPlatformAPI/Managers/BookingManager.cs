using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Entities;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Models;

namespace CleaningPlatformAPI.Managers;

public class BookingManager
{
    private const int MaxRetryAttempts = 3;

    private readonly AppDbContext _db;
    private readonly AvailabilityManager _availability;
    private readonly SopManager _sopManager;
    private readonly InventoryManager _inventoryManager;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public BookingManager(AppDbContext db, AvailabilityManager availability, SopManager sopManager, InventoryManager inventoryManager, IStringLocalizer<SharedResources> localizer) { _db = db; _availability = availability; _sopManager = sopManager; _inventoryManager = inventoryManager;
            _localizer = localizer;}

    public async Task<List<BookingResponse>> GetBookingsAsync(DateTime date, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .Where(b => b.ScheduledDate.Date == date.Date)
            .ToListAsync(ct);
        return bookings.Select(BookingMapper.ToResponse).ToList();
    }

    public async Task<Paginated<BookingResponse>> GetAllBookingsAsync(
        PaginationParams pagination,
        string? statusFilter = null,
        CancellationToken ct = default)
    {
        var query = _db.BookingViews.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var term = pagination.Search.Trim().ToLower();
            query = query.Where(b => b.ClientName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(b => b.Status == statusFilter);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.ScheduledDate)
            .ThenByDescending(b => b.BookingId)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(ct);

        var mapped = items.Select(b => BookingMapper.ToResponse(b, b.ClientId)).ToList();
        return new Paginated<BookingResponse>(mapped, totalCount);
    }

    public async Task<List<BookingResponse>> GetAssignedBookingsForEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .Where(b => b.Assignments.Any(a => a.EmployeeId == employeeId))
            .OrderByDescending(b => b.ScheduledDate)
            .ToListAsync(ct);
        return bookings.Select(BookingMapper.ToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetAssignedBookingsForEmployeeByDateAsync(int employeeId, DateTime date, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .Where(b => b.Assignments.Any(a => a.EmployeeId == employeeId) && b.ScheduledDate.Date == date.Date)
            .OrderBy(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);
        return bookings.Select(BookingMapper.ToResponse).ToList();
    }

    public async Task<BookingResponse> GetBookingDetailByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
                .ThenInclude(sc => sc.InventoryRequirements).ThenInclude(r => r.Inventory)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{id} was not found.", 404);
        return BookingMapper.ToDetailResponse(booking);
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest dto, CancellationToken ct = default)
    {
        var dateStr = dto.Date.ToString("dd MMM yyyy");

        // 1. Validate slot availability
        var slots = await _availability.GetSlotsAsync(dto.Date, ct);
        var slot = slots.FirstOrDefault(s => s.Hour == dto.Hour);

        if (slot is null || slot.IsClosed)
            throw new AppException("SLOT_CLOSED", $"The {dto.Hour}:00 slot on {dateStr} is closed and not available for booking.", 422);

        if (slot.Available <= 0)
            throw new AppException("SLOT_FULLY_BOOKED", $"The {dto.Hour}:00 slot on {dateStr} is fully booked ({slot.Booked}/{slot.Capacity} spots taken).", 422);

        var customerName = dto.CustomerName.Trim();
        var phone = dto.Phone.Trim();
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

        if (string.IsNullOrWhiteSpace(customerName))
            throw new AppException("CUSTOMER_NAME_REQUIRED", _localizer["err_customer_name_required"], 422);
        if (string.IsNullOrWhiteSpace(phone))
            throw new AppException("PHONE_REQUIRED", "Phone number is required.", 422);

        var now = DateTime.UtcNow;

        for (var attempt = 1; ; attempt++)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                // Re-check availability inside the transaction to prevent overbooking under concurrency
                var actualBooked = await _db.Bookings
                    .CountAsync(b => b.ScheduledDate.Date == dto.Date.Date
                        && b.ScheduledTimeSlot == TimeSpan.FromHours(dto.Hour)
                        && b.Status != BookingStatus.Cancelled, ct);
                if (slot.Capacity - actualBooked <= 0)
                    throw new AppException("SLOT_FULLY_BOOKED", $"The {dto.Hour}:00 slot on {dateStr} is fully booked ({actualBooked}/{slot.Capacity} spots taken).", 422);

                // 2. Client deduplication – search by phone or email in Contacts table
                Client? client = null;

                // First, try to find client by phone number
                client = await _db.Clients
                    .Include(c => c.Contacts)
                    .FirstOrDefaultAsync(c => c.Contacts.Any(ct => ct.Phone == phone && ct.IsActive), ct);

                // If not found, try by email
                if (client is null && !string.IsNullOrWhiteSpace(email))
                {
                    client = await _db.Clients
                        .Include(c => c.Contacts)
                        .FirstOrDefaultAsync(c => c.Contacts.Any(ct => ct.Email == email && ct.IsActive), ct);
                }

                if (client is not null)
                {
                    // Update existing client info (name may have changed)
                    client.ClientName = customerName;
                    client.UpdatedAt = now;

                    // Update the primary contact (or add if missing)
                    var primaryContact = client.Contacts.FirstOrDefault(c => c.IsPrimary);
                    if (primaryContact is not null)
                    {
                        primaryContact.ContactName = customerName;
                        primaryContact.Phone = phone;
                        if (!string.IsNullOrWhiteSpace(email))
                            primaryContact.Email = email;
                        primaryContact.UpdatedAt = now;
                    }
                    else
                    {
                        // Should not happen, but fallback: add a primary contact
                        client.Contacts.Add(new Contact
                        {
                            ContactName = customerName,
                            Phone = phone,
                            Email = email,
                            IsPrimary = true,
                            IsActive = true,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }
                }
                else
                {
                    // Create a new client – type is "Person", not "OneTime"
                    client = new Client
                    {
                        ClientName = customerName,
                        Type = "Person",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        Contacts =
                    [
                        new Contact
                        {
                            ContactName = customerName,
                            Phone = phone,
                            Email = email,
                            IsPrimary = true,
                            IsActive = true,
                            CreatedAt = now,
                            UpdatedAt = now
                        }
                    ]
                    };
                    _db.Clients.Add(client);
                    // Save to get the client ID before creating the booking
                    await _db.SaveChangesAsync(ct);
                }

                // 3. Load the selected service catalog entry
                var catalogEntry = await _db.ServiceCatalog
                    .FirstOrDefaultAsync(s => s.Id == dto.ServiceCatalogId && s.IsActive, ct);
                if (catalogEntry is null)
                    throw new AppException("SERVICE_NOT_FOUND", "Selected service was not found or is no longer available.", 404);

                if (!Enum.TryParse<BookingServiceType>(catalogEntry.ServiceType, true, out var serviceType))
                    throw new AppException("INVALID_SERVICE_TYPE", $"Invalid service type '{catalogEntry.ServiceType}'.", 422);

                // 4. Create the booking
                var booking = new Booking
                {
                    ClientId = client.Id,
                    ServiceType = serviceType,
                    ScheduledDate = dto.Date.Date,
                    ScheduledTimeSlot = TimeSpan.FromHours(dto.Hour),
                    Status = BookingStatus.Pending,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                booking.BookingServices.Add(new BookingService
                {
                    ServiceCatalogId = catalogEntry.Id,
                    EstimatedPrice = catalogEntry.BasePrice,
                    Quantity = 1
                });

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // 5. Return the response using your mapper
                return BookingMapper.ToResponse(booking);
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts && SqlHelper.IsDeadlock(ex))
            {
                await transaction.RollbackAsync(ct);
                continue;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    public async Task<BookingResponse> CreateAdminBookingAsync(CreateAdminBookingRequest dto, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == dto.ClientId && c.IsActive, ct);
        if (!clientExists)
            throw new AppException("CLIENT_NOT_FOUND_INACTIVE", $"Client #{dto.ClientId} was not found or is inactive.", 404);

        if (dto.SiteId.HasValue)
        {
            var siteOk = await _db.Sites.AnyAsync(s => s.Id == dto.SiteId.Value && s.ClientId == dto.ClientId && s.IsActive, ct);
            if (!siteOk)
                throw new AppException("SITE_NOT_FOUND_INACTIVE", $"Site #{dto.SiteId} does not belong to this client or is inactive.", 404);
        }

        var dateStr = dto.Date.ToString("dd MMM yyyy");
        var slots   = await _availability.GetSlotsAsync(dto.Date, ct);
        var slot    = slots.FirstOrDefault(s => s.Hour == dto.Hour);

        if (slot is null || slot.IsClosed)
            throw new AppException("SLOT_CLOSED", $"The {dto.Hour}:00 slot on {dateStr} is closed.", 422);
        if (slot.Available <= 0)
            throw new AppException("SLOT_FULLY_BOOKED", $"The {dto.Hour}:00 slot on {dateStr} is fully booked ({slot.Booked}/{slot.Capacity} spots taken).", 422);

        var now = DateTime.UtcNow;

        for (var attempt = 1; ; attempt++)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                // Re-check availability inside the transaction
                var actualBooked = await _db.Bookings
                    .CountAsync(b => b.ScheduledDate.Date == dto.Date.Date
                        && b.ScheduledTimeSlot == TimeSpan.FromHours(dto.Hour)
                        && b.Status != BookingStatus.Cancelled, ct);
                if (slot.Capacity - actualBooked <= 0)
                    throw new AppException("SLOT_FULLY_BOOKED", $"The {dto.Hour}:00 slot on {dateStr} is fully booked ({actualBooked}/{slot.Capacity} spots taken).", 422);

                var booking = new Booking
                {
                    ClientId          = dto.ClientId,
                    SiteId            = dto.SiteId,
                    ServiceType       = dto.ServiceType,
                    ScheduledDate     = dto.Date.Date,
                    ScheduledTimeSlot = TimeSpan.FromHours(dto.Hour),
                    Status            = BookingStatus.Pending,
                    Notes             = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                    CreatedAt         = now,
                    UpdatedAt         = now
                };

                foreach (var service in dto.Services.Where(s => s.ServiceCatalogId > 0))
                {
                    booking.BookingServices.Add(new BookingService
                    {
                        ServiceCatalogId = service.ServiceCatalogId,
                        EstimatedPrice   = service.EstimatedPrice,
                        FinalPrice       = service.FinalPrice,
                        Quantity         = service.Quantity <= 0 ? 1 : service.Quantity,
                        Notes            = string.IsNullOrWhiteSpace(service.Notes) ? null : service.Notes.Trim()
                    });
                }

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                var templates = await _sopManager.GetDefaultTemplatesForServiceTypeAsync(dto.ServiceType.ToString(), ct);
                foreach (var template in templates)
                    await _sopManager.AssignSopToBookingAsync(booking.Id, new AssignSopRequest { SopTemplateId = template.Id }, ct);

                return await GetBookingDetailByIdAsync(booking.Id, ct);
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts && SqlHelper.IsDeadlock(ex))
            {
                await transaction.RollbackAsync(ct);
                continue;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    public async Task<BookingResponse> UpdateStatusAsync(int id, string status, CancellationToken ct = default)
    {
        if (!Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            throw new AppException("INVALID_BOOKING_STATUS", $"'{status}' is not a valid booking status. Valid values: Pending, InProgress, Completed, Cancelled.", 422);

        var booking = await _db.Bookings
            .Include(b => b.Client)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{id} was not found.", 404);

        var wasCancelled = booking.Status != BookingStatus.Cancelled && bookingStatus == BookingStatus.Cancelled;

        booking.Status    = bookingStatus;
        booking.UpdatedAt = DateTime.UtcNow;
        if (bookingStatus == BookingStatus.Completed)
            booking.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        if (wasCancelled)
            await RestoreAllConsumablesForBookingAsync(id, ct);

        return BookingMapper.ToResponse(booking);
    }

    public async Task<BookingResponse> AddAssignmentAsync(int bookingId, int employeeId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{bookingId} was not found.", 404);

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            throw new AppException("INVALID_BOOKING_FOR_ASSIGN", $"Cannot assign employees to a booking with status '{booking.Status}'. Only Pending or InProgress bookings can be assigned.", 422);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive, ct);
        if (employee is null)
            throw new AppException("EMPLOYEE_NOT_FOUND", $"Employee #{employeeId} was not found or is inactive.", 404);

        var alreadyAssigned = await _db.BookingAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.EmployeeId == employeeId, ct);
        if (alreadyAssigned)
            throw new AppException("EMPLOYEE_ALREADY_ASSIGNED", $"{employee.FirstName} {employee.LastName} is already assigned to this booking.", 409);

        _db.BookingAssignments.Add(new BookingAssignment
        {
            BookingId  = bookingId,
            EmployeeId = employeeId,
            AssignedAt = DateTime.UtcNow
        });
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetBookingDetailByIdAsync(bookingId, ct);
    }

    public async Task RemoveAssignmentAsync(int bookingId, int employeeId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{bookingId} was not found.", 404);

        if (booking.Status == BookingStatus.InProgress || booking.Status == BookingStatus.Completed)
            throw new AppException("INVALID_BOOKING_FOR_REMOVE", $"Cannot remove an assignment from a booking with status '{booking.Status}'.", 422);

        var assignment = await _db.BookingAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.BookingId == bookingId, ct);
        if (assignment is null)
            throw new AppException("EMPLOYEE_NOT_ASSIGNED", $"Employee #{employeeId} is not assigned to booking #{bookingId}.", 404);

        _db.BookingAssignments.Remove(assignment);
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return;
    }

    public async Task<BookingResponse> AddServiceAsync(int bookingId, int serviceCatalogId, decimal? estimatedPrice, decimal quantity, decimal? finalPrice = null, string? notes = null, CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw new AppException("QUANTITY_REQUIRED", _localizer["err_quantity_required"], 422);

        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{bookingId} was not found.", 404);

        var hasInvoice = await _db.InvoiceBookings.AnyAsync(ib => ib.BookingId == bookingId, ct);
        if (hasInvoice)
            throw new AppException("INVOICE_ALREADY_EXISTS", "Cannot add services after an invoice has been generated for this booking. Record any changes on the invoice directly.", 409);

        var serviceExists = await _db.ServiceCatalog.AnyAsync(s => s.Id == serviceCatalogId, ct);
        if (!serviceExists)
            throw new AppException("CATALOG_SERVICE_NOT_FOUND", $"Service #{serviceCatalogId} was not found in the catalog.", 404);

        _db.BookingServices.Add(new BookingService
        {
            BookingId        = bookingId,
            ServiceCatalogId = serviceCatalogId,
            EstimatedPrice   = estimatedPrice,
            FinalPrice       = finalPrice,
            Quantity         = quantity,
            Notes            = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        });
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await DeductConsumablesForServiceAsync(serviceCatalogId, ct);

        return await GetBookingDetailByIdAsync(bookingId, ct);
    }

    public async Task RemoveServiceAsync(int bookingId, int serviceCatalogId, CancellationToken ct = default)
    {
        var hasInvoice = await _db.InvoiceBookings.AnyAsync(ib => ib.BookingId == bookingId, ct);
        if (hasInvoice)
            throw new AppException("CANNOT_REMOVE_AFTER_INVOICE", "Cannot remove services after an invoice has been generated for this booking.", 409);

        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.ServiceCatalogId == serviceCatalogId && bs.BookingId == bookingId, ct);
        if (bookingService is null)
            throw new AppException("SERVICE_NOT_ON_BOOKING", $"Service #{serviceCatalogId} was not found on booking #{bookingId}.", 404);

        await RestoreConsumablesForServiceAsync(bookingService.ServiceCatalogId, ct);

        _db.BookingServices.Remove(bookingService);
        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is not null) booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return;
    }

    public async Task<BookingResponse> UpdateServicePriceAsync(int bookingId, int serviceCatalogId, decimal? finalPrice, CancellationToken ct = default)
    {
        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.ServiceCatalogId == serviceCatalogId && bs.BookingId == bookingId, ct);
        if (bookingService is null)
            throw new AppException("SERVICE_NOT_ON_BOOKING", $"Service #{serviceCatalogId} was not found on booking #{bookingId}.", 404);

        bookingService.FinalPrice = finalPrice;
        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is not null) booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetBookingDetailByIdAsync(bookingId, ct);
    }

    private async Task DeductConsumablesForServiceAsync(int serviceCatalogId, CancellationToken ct)
    {
        var requirements = await _db.ServiceInventoryRequirements
            .Include(r => r.Inventory)
            .Where(r => r.ServiceCatalogId == serviceCatalogId && r.Inventory.Type == "Consumable")
            .ToListAsync(ct);
        foreach (var req in requirements)
            await _inventoryManager.AdjustStockAsync(req.InventoryId, -req.QuantityNeeded, ct);
    }

    private async Task RestoreConsumablesForServiceAsync(int serviceCatalogId, CancellationToken ct)
    {
        var requirements = await _db.ServiceInventoryRequirements
            .Include(r => r.Inventory)
            .Where(r => r.ServiceCatalogId == serviceCatalogId && r.Inventory.Type == "Consumable")
            .ToListAsync(ct);
        foreach (var req in requirements)
            await _inventoryManager.AdjustStockAsync(req.InventoryId, req.QuantityNeeded, ct);
    }

    private async Task RestoreAllConsumablesForBookingAsync(int bookingId, CancellationToken ct)
    {
        var serviceIds = await _db.BookingServices
            .Where(bs => bs.BookingId == bookingId)
            .Select(bs => bs.ServiceCatalogId)
            .ToListAsync(ct);
        foreach (var serviceId in serviceIds)
            await RestoreConsumablesForServiceAsync(serviceId, ct);
    }
}
