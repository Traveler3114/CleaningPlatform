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
    private readonly AppDbContext _db;
    private readonly AvailabilityManager _availability;

    public BookingManager(AppDbContext db, AvailabilityManager availability)
    {
        _db = db;
        _availability = availability;
    }

    public async Task<List<BookingResponse>> GetBookingsAsync(DateTime date, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client)
                .ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments)
                .ThenInclude(a => a.Employee)
                    .ThenInclude(e => e.Role)
            .Where(b => b.ScheduledDate.Date == date.Date)
            .ToListAsync(ct);

        return bookings.Select(BookingMapper.ToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetAllBookingsAsync(CancellationToken ct = default)
    {
        var bookings = await _db.BookingViews
            .AsNoTracking()
            .Join(
                _db.Bookings.AsNoTracking(),
                view => view.BookingId,
                booking => booking.Id,
                (view, booking) => new { View = view, booking.ClientId })
            .OrderByDescending(b => b.View.ScheduledDate)
            .ToListAsync(ct);

        return bookings.Select(b => BookingMapper.ToResponse(b.View, b.ClientId)).ToList();
    }

    public async Task<List<BookingResponse>> GetAssignedBookingsForEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client)
                .ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments)
                .ThenInclude(a => a.Employee)
                    .ThenInclude(e => e.Role)
            .Where(b => b.Assignments.Any(a => a.EmployeeId == employeeId))
            .OrderByDescending(b => b.ScheduledDate)
            .ToListAsync(ct);

        return bookings.Select(BookingMapper.ToResponse).ToList();
    }

    public async Task<BookingResponse?> GetBookingDetailByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Client)
                .ThenInclude(c => c.Contacts)
            .Include(b => b.Assignments)
                .ThenInclude(a => a.Employee)
                    .ThenInclude(e => e.Role)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : BookingMapper.ToDetailResponse(booking);
    }

    public async Task<OperationResult<BookingResponse>> CreateBookingAsync(CreateBookingRequest dto, CancellationToken ct = default)
    {
        var slots = await _availability.GetSlotsAsync(dto.Date, ct);
        var slot = slots.FirstOrDefault(s => s.Hour == dto.Hour);
        if (slot == null || slot.IsClosed)
            return OperationResult<BookingResponse>.Fail("Slot is closed or unavailable.");
        if (slot.Available <= 0)
            return OperationResult<BookingResponse>.Fail("No capacity available for this slot.");

        var customerName = dto.CustomerName.Trim();
        var phone = dto.Phone.Trim();
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(phone))
            return OperationResult<BookingResponse>.Fail("Customer name and phone are required.");

        var now = DateTime.UtcNow;
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var client = new Client
            {
                ClientName = customerName,
                Type = "OneTime",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                Contacts =
                [
                    new Contact
                    {
                        ContactName = customerName,
                        Phone = phone,
                        IsPrimary = true,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                ]
            };

            var booking = new Booking
            {
                Client = client,
                ServiceType = BookingServiceType.Vehicle,
                ScheduledDate = dto.Date.Date,
                ScheduledTimeSlot = TimeSpan.FromHours(dto.Hour),
                Status = BookingStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return OperationResult<BookingResponse>.Ok(BookingMapper.ToResponse(booking));
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<OperationResult<BookingResponse>> UpdateStatusAsync(int id, string status, CancellationToken ct = default)
    {
        if (!Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            return OperationResult<BookingResponse>.Fail("Invalid status.");

        var booking = await _db.Bookings
            .Include(b => b.Client)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments)
                .ThenInclude(a => a.Employee)
                    .ThenInclude(e => e.Role)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking == null)
            return OperationResult<BookingResponse>.Fail("Booking not found.");

        booking.Status = bookingStatus;
        if (bookingStatus == BookingStatus.Completed)
            booking.CompletedAt = DateTime.UtcNow;

        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<BookingResponse>.Ok(BookingMapper.ToResponse(booking));
    }

    public async Task<OperationResult<BookingResponse>> AddAssignmentAsync(int bookingId, int employeeId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking == null)
            return OperationResult<BookingResponse>.Fail("Booking not found.");

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            return OperationResult<BookingResponse>.Fail("Cannot assign employees to completed or cancelled bookings.");

        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive, ct);
        if (!employeeExists)
            return OperationResult<BookingResponse>.Fail("Employee not found or inactive.");

        var alreadyAssigned = await _db.BookingAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.EmployeeId == employeeId, ct);
        if (alreadyAssigned)
            return OperationResult<BookingResponse>.Fail("Employee already assigned.");

        _db.BookingAssignments.Add(new BookingAssignment
        {
            BookingId = bookingId,
            EmployeeId = employeeId,
            AssignedAt = DateTime.UtcNow
        });

        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var detail = await GetBookingDetailByIdAsync(bookingId, ct);
        return detail == null
            ? OperationResult<BookingResponse>.Fail("Booking not found.")
            : OperationResult<BookingResponse>.Ok(detail);
    }

    public async Task<OperationResult<string>> RemoveAssignmentAsync(int bookingId, int assignmentId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking == null)
            return OperationResult<string>.Fail("Booking not found.");

        if (booking.Status == BookingStatus.InProgress || booking.Status == BookingStatus.Completed)
            return OperationResult<string>.Fail("Cannot remove assignment from an in-progress or completed booking.");

        var assignment = await _db.BookingAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.BookingId == bookingId, ct);
        if (assignment == null)
            return OperationResult<string>.Fail("Assignment not found.");

        _db.BookingAssignments.Remove(assignment);
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Assignment removed.");
    }

    public async Task<OperationResult<BookingResponse>> AddServiceAsync(
        int bookingId,
        int serviceCatalogId,
        decimal? estimatedPrice,
        decimal quantity,
        decimal? finalPrice = null,
        string? notes = null,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
            return OperationResult<BookingResponse>.Fail("Quantity must be greater than zero.");

        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking == null)
            return OperationResult<BookingResponse>.Fail("Booking not found.");

        var hasInvoice = await _db.InvoiceBookings.AnyAsync(ib => ib.BookingId == bookingId, ct);
        if (hasInvoice)
            return OperationResult<BookingResponse>.Fail("Cannot modify booking services after an invoice has been linked.");

        var serviceExists = await _db.ServiceCatalog.AnyAsync(s => s.Id == serviceCatalogId, ct);
        if (!serviceExists)
            return OperationResult<BookingResponse>.Fail("Service not found.");

        _db.BookingServices.Add(new BookingService
        {
            BookingId = bookingId,
            ServiceCatalogId = serviceCatalogId,
            EstimatedPrice = estimatedPrice,
            FinalPrice = finalPrice,
            Quantity = quantity,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        });

        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var detail = await GetBookingDetailByIdAsync(bookingId, ct);
        return detail == null
            ? OperationResult<BookingResponse>.Fail("Booking not found.")
            : OperationResult<BookingResponse>.Ok(detail);
    }

    public async Task<OperationResult<string>> RemoveServiceAsync(int bookingId, int bookingServiceId, CancellationToken ct = default)
    {
        var hasInvoice = await _db.InvoiceBookings.AnyAsync(ib => ib.BookingId == bookingId, ct);
        if (hasInvoice)
            return OperationResult<string>.Fail("Cannot modify booking services after an invoice has been linked.");

        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == bookingServiceId && bs.BookingId == bookingId, ct);

        if (bookingService == null)
            return OperationResult<string>.Fail("Booking service not found.");

        _db.BookingServices.Remove(bookingService);

        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking != null)
            booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Service removed.");
    }

    public async Task<OperationResult<BookingResponse>> UpdateServicePriceAsync(int bookingId, int bookingServiceId, decimal? finalPrice, CancellationToken ct = default)
    {
        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == bookingServiceId && bs.BookingId == bookingId, ct);

        if (bookingService == null)
            return OperationResult<BookingResponse>.Fail("Booking service not found.");

        bookingService.FinalPrice = finalPrice;

        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking != null)
            booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var detail = await GetBookingDetailByIdAsync(bookingId, ct);
        return detail == null
            ? OperationResult<BookingResponse>.Fail("Booking not found.")
            : OperationResult<BookingResponse>.Ok(detail);
    }
}
