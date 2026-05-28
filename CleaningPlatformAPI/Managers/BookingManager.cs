using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class BookingManager
{
    private const int MaxRetryAttempts = 3;

    private readonly AppDbContext _db;
    private readonly AvailabilityManager _availability;
    private readonly SopManager _sopManager;

    public BookingManager(AppDbContext db, AvailabilityManager availability, SopManager sopManager) { _db = db; _availability = availability; _sopManager = sopManager; }

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

    public async Task<PagedResult<BookingResponse>> GetAllBookingsAsync(
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
        return PagedResult<BookingResponse>.From(mapped, totalCount, pagination.Page, pagination.PageSize);
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

    public async Task<OperationResult<BookingResponse>> GetBookingDetailByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        return booking is null
            ? OperationResult<BookingResponse>.Fail($"Booking #{id} was not found.")
            : OperationResult<BookingResponse>.Ok(BookingMapper.ToDetailResponse(booking));
    }

    public async Task<OperationResult<BookingResponse>> CreateBookingAsync(CreateBookingRequest dto, CancellationToken ct = default)
    {
        var dateStr = dto.Date.ToString("dd MMM yyyy");

        // 1. Validate slot availability
        var slots = await _availability.GetSlotsAsync(dto.Date, ct);
        var slot = slots.FirstOrDefault(s => s.Hour == dto.Hour);

        if (slot is null || slot.IsClosed)
            return OperationResult<BookingResponse>.Fail($"The {dto.Hour}:00 slot on {dateStr} is closed and not available for booking.");

        if (slot.Available <= 0)
            return OperationResult<BookingResponse>.Fail($"The {dto.Hour}:00 slot on {dateStr} is fully booked ({slot.Booked}/{slot.Capacity} spots taken).");

        var customerName = dto.CustomerName.Trim();
        var phone = dto.Phone.Trim();
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

        if (string.IsNullOrWhiteSpace(customerName))
            return OperationResult<BookingResponse>.Fail("Customer name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            return OperationResult<BookingResponse>.Fail("Phone number is required.");

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
                    return OperationResult<BookingResponse>.Fail($"The {dto.Hour}:00 slot on {dateStr} is fully booked ({actualBooked}/{slot.Capacity} spots taken).");

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
                        Contacts = new List<Contact>
                    {
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
                    }
                    };
                    _db.Clients.Add(client);
                    // Save to get the client ID before creating the booking
                    await _db.SaveChangesAsync(ct);
                }

                // 3. Load the selected service catalog entry
                var catalogEntry = await _db.ServiceCatalog
                    .FirstOrDefaultAsync(s => s.Id == dto.ServiceCatalogId && s.IsActive, ct);
                if (catalogEntry is null)
                    return OperationResult<BookingResponse>.Fail("Selected service was not found or is no longer available.");

                if (!Enum.TryParse<BookingServiceType>(catalogEntry.ServiceType, true, out var serviceType))
                    return OperationResult<BookingResponse>.Fail($"Invalid service type '{catalogEntry.ServiceType}'.");

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
                    EstimatedPrice = catalogEntry.PriceAvg,
                    Quantity = 1
                });

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // 5. Return the response using your mapper
                return OperationResult<BookingResponse>.Ok(BookingMapper.ToResponse(booking));
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

    public async Task<OperationResult<BookingResponse>> CreateAdminBookingAsync(CreateAdminBookingRequest dto, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == dto.ClientId && c.IsActive, ct);
        if (!clientExists)
            return OperationResult<BookingResponse>.Fail($"Client #{dto.ClientId} was not found or is inactive.");

        if (dto.SiteId.HasValue)
        {
            var siteOk = await _db.Sites.AnyAsync(s => s.Id == dto.SiteId.Value && s.ClientId == dto.ClientId && s.IsActive, ct);
            if (!siteOk)
                return OperationResult<BookingResponse>.Fail($"Site #{dto.SiteId} does not belong to this client or is inactive.");
        }

        var dateStr = dto.Date.ToString("dd MMM yyyy");
        var slots   = await _availability.GetSlotsAsync(dto.Date, ct);
        var slot    = slots.FirstOrDefault(s => s.Hour == dto.Hour);

        if (slot is null || slot.IsClosed)
            return OperationResult<BookingResponse>.Fail($"The {dto.Hour}:00 slot on {dateStr} is closed.");
        if (slot.Available <= 0)
            return OperationResult<BookingResponse>.Fail($"The {dto.Hour}:00 slot on {dateStr} is fully booked ({slot.Booked}/{slot.Capacity} spots taken).");

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
                    return OperationResult<BookingResponse>.Fail($"The {dto.Hour}:00 slot on {dateStr} is fully booked ({actualBooked}/{slot.Capacity} spots taken).");

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

                var detail = await GetBookingDetailByIdAsync(booking.Id, ct);
                return detail.Success ? detail : OperationResult<BookingResponse>.Fail("Booking was created but could not be loaded. Please refresh.");
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

    public async Task<OperationResult<BookingResponse>> UpdateStatusAsync(int id, string status, CancellationToken ct = default)
    {
        if (!Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            return OperationResult<BookingResponse>.Fail($"'{status}' is not a valid booking status. Valid values: Pending, Confirmed, InProgress, Completed, Cancelled.");

        var booking = await _db.Bookings
            .Include(b => b.Client)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
            return OperationResult<BookingResponse>.Fail($"Booking #{id} was not found.");

        booking.Status    = bookingStatus;
        booking.UpdatedAt = DateTime.UtcNow;
        if (bookingStatus == BookingStatus.Completed)
            booking.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<BookingResponse>.Ok(BookingMapper.ToResponse(booking));
    }

    public async Task<OperationResult<BookingResponse>> AddAssignmentAsync(int bookingId, int employeeId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null)
            return OperationResult<BookingResponse>.Fail($"Booking #{bookingId} was not found.");

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            return OperationResult<BookingResponse>.Fail($"Cannot assign employees to a booking with status '{booking.Status}'. Only Pending, Confirmed, or InProgress bookings can be assigned.");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive, ct);
        if (employee is null)
            return OperationResult<BookingResponse>.Fail($"Employee #{employeeId} was not found or is inactive.");

        var alreadyAssigned = await _db.BookingAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.EmployeeId == employeeId, ct);
        if (alreadyAssigned)
            return OperationResult<BookingResponse>.Fail($"{employee.FirstName} {employee.LastName} is already assigned to this booking.");

        _db.BookingAssignments.Add(new BookingAssignment
        {
            BookingId  = bookingId,
            EmployeeId = employeeId,
            AssignedAt = DateTime.UtcNow
        });
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var detail = await GetBookingDetailByIdAsync(bookingId, ct);
        return detail;
    }

    public async Task<OperationResult<string>> RemoveAssignmentAsync(int bookingId, int assignmentId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null)
            return OperationResult<string>.Fail($"Booking #{bookingId} was not found.");

        if (booking.Status == BookingStatus.InProgress || booking.Status == BookingStatus.Completed)
            return OperationResult<string>.Fail($"Cannot remove an assignment from a booking with status '{booking.Status}'.");

        var assignment = await _db.BookingAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.BookingId == bookingId, ct);
        if (assignment is null)
            return OperationResult<string>.Fail($"Assignment #{assignmentId} was not found for booking #{bookingId}.");

        _db.BookingAssignments.Remove(assignment);
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Assignment removed.");
    }

    public async Task<OperationResult<BookingResponse>> AddServiceAsync(int bookingId, int serviceCatalogId, decimal? estimatedPrice, decimal quantity, decimal? finalPrice = null, string? notes = null, CancellationToken ct = default)
    {
        if (quantity <= 0)
            return OperationResult<BookingResponse>.Fail("Quantity must be greater than zero.");

        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is null)
            return OperationResult<BookingResponse>.Fail($"Booking #{bookingId} was not found.");

        var hasInvoice = await _db.InvoiceBookings.AnyAsync(ib => ib.BookingId == bookingId, ct);
        if (hasInvoice)
            return OperationResult<BookingResponse>.Fail("Cannot add services after an invoice has been generated for this booking. Record any changes on the invoice directly.");

        var serviceExists = await _db.ServiceCatalog.AnyAsync(s => s.Id == serviceCatalogId, ct);
        if (!serviceExists)
            return OperationResult<BookingResponse>.Fail($"Service #{serviceCatalogId} was not found in the catalog.");

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

        var detail = await GetBookingDetailByIdAsync(bookingId, ct);
        return detail;
    }

    public async Task<OperationResult<string>> RemoveServiceAsync(int bookingId, int bookingServiceId, CancellationToken ct = default)
    {
        var hasInvoice = await _db.InvoiceBookings.AnyAsync(ib => ib.BookingId == bookingId, ct);
        if (hasInvoice)
            return OperationResult<string>.Fail("Cannot remove services after an invoice has been generated for this booking.");

        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == bookingServiceId && bs.BookingId == bookingId, ct);
        if (bookingService is null)
            return OperationResult<string>.Fail($"Service #{bookingServiceId} was not found on booking #{bookingId}.");

        _db.BookingServices.Remove(bookingService);
        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is not null) booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Service removed.");
    }

    public async Task<OperationResult<BookingResponse>> UpdateServicePriceAsync(int bookingId, int bookingServiceId, decimal? finalPrice, CancellationToken ct = default)
    {
        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == bookingServiceId && bs.BookingId == bookingId, ct);
        if (bookingService is null)
            return OperationResult<BookingResponse>.Fail($"Service #{bookingServiceId} was not found on booking #{bookingId}.");

        bookingService.FinalPrice = finalPrice;
        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is not null) booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var detail = await GetBookingDetailByIdAsync(bookingId, ct);
        return detail;
    }
}