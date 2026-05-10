// CleaningPlatformAPI/Managers/BookingManager.cs

using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;

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

    public async Task<List<BookingDto>> GetBookingsAsync(DateTime date)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client)
            .ThenInclude(c => c.Contacts)
            .Where(b => b.ScheduledDate.Date == date.Date)
            .ToListAsync();
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _db.Bookings
            .Include(b => b.Client)
            .ThenInclude(c => c.Contacts)
            .OrderByDescending(b => b.ScheduledDate)
            .ToListAsync();
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<BookingDetailDto?> GetBookingDetailByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Client)
                .ThenInclude(c => c.Contacts)
            .Include(b => b.AssignedEmployee)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.Id == id);

        return booking is null ? null : MapToDetailDto(booking);
    }

    public async Task<OperationResult<BookingDto>> CreateBookingAsync(CreateBookingDto dto)
    {
        var slots = await _availability.GetSlotsAsync(dto.Date);
        var slot = slots.FirstOrDefault(s => s.Hour == dto.Hour);
        if (slot == null || slot.IsClosed)
            return OperationResult<BookingDto>.Fail("Slot is closed or unavailable.");
        if (slot.Available <= 0)
            return OperationResult<BookingDto>.Fail("No capacity available for this slot.");

        var customerName = dto.CustomerName.Trim();
        var phone = dto.Phone.Trim();
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(phone))
            return OperationResult<BookingDto>.Fail("Customer name and phone are required.");

        var now = DateTime.UtcNow;
        var client = new Client
        {
            ClientName = customerName,
            Type = "OneTime",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Contacts = new List<Contact>
            {
                new()
                {
                    ContactName = customerName,
                    Phone = phone,
                    IsPrimary = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            }
        };

        var booking = new Booking
        {
            Client = client,
            ServiceType = "Vehicle",
            ScheduledDate = dto.Date.Date,
            ScheduledTimeSlot = TimeSpan.FromHours(dto.Hour),
            Status = BookingStatus.Pending.ToString(),
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return OperationResult<BookingDto>.Ok(MapToDto(booking));
    }

    public async Task<OperationResult<BookingDto>> UpdateStatusAsync(int id, string status)
    {
        if (!Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            return OperationResult<BookingDto>.Fail("Invalid status.");
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null)
            return OperationResult<BookingDto>.Fail("Booking not found.");
        booking.Status = bookingStatus.ToString();
        if (bookingStatus == BookingStatus.Completed)
            booking.CompletedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return OperationResult<BookingDto>.Ok(MapToDto(booking));
    }

    public async Task<OperationResult<BookingDetailDto>> AssignEmployeeAsync(int bookingId, int? employeeId)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking == null)
            return OperationResult<BookingDetailDto>.Fail("Booking not found.");

        if (employeeId.HasValue)
        {
            var employeeExists = await _db.Employees.AnyAsync(e => e.Id == employeeId.Value && e.IsActive);
            if (!employeeExists)
                return OperationResult<BookingDetailDto>.Fail("Employee not found or inactive.");
        }

        booking.AssignedEmployeeId = employeeId;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var detail = await GetBookingDetailByIdAsync(bookingId);
        return detail == null
            ? OperationResult<BookingDetailDto>.Fail("Booking not found.")
            : OperationResult<BookingDetailDto>.Ok(detail);
    }

    public async Task<OperationResult<BookingDetailDto>> AddServiceAsync(
        int bookingId,
        int serviceCatalogId,
        decimal? estimatedPrice,
        decimal quantity,
        decimal? finalPrice = null,
        string? notes = null)
    {
        if (quantity <= 0)
            return OperationResult<BookingDetailDto>.Fail("Quantity must be greater than zero.");

        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking == null)
            return OperationResult<BookingDetailDto>.Fail("Booking not found.");

        var serviceExists = await _db.ServiceCatalog.AnyAsync(s => s.Id == serviceCatalogId);
        if (!serviceExists)
            return OperationResult<BookingDetailDto>.Fail("Service not found.");

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
        await _db.SaveChangesAsync();

        var detail = await GetBookingDetailByIdAsync(bookingId);
        return detail == null
            ? OperationResult<BookingDetailDto>.Fail("Booking not found.")
            : OperationResult<BookingDetailDto>.Ok(detail);
    }

    public async Task<OperationResult<string>> RemoveServiceAsync(int bookingId, int bookingServiceId)
    {
        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == bookingServiceId && bs.BookingId == bookingId);

        if (bookingService == null)
            return OperationResult<string>.Fail("Booking service not found.");

        _db.BookingServices.Remove(bookingService);

        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking != null)
            booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<string>.Ok("Service removed.");
    }

    public async Task<OperationResult<BookingDetailDto>> UpdateServicePriceAsync(int bookingId, int bookingServiceId, decimal? finalPrice)
    {
        var bookingService = await _db.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == bookingServiceId && bs.BookingId == bookingId);

        if (bookingService == null)
            return OperationResult<BookingDetailDto>.Fail("Booking service not found.");

        bookingService.FinalPrice = finalPrice;

        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking != null)
            booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var detail = await GetBookingDetailByIdAsync(bookingId);
        return detail == null
            ? OperationResult<BookingDetailDto>.Fail("Booking not found.")
            : OperationResult<BookingDetailDto>.Ok(detail);
    }

    public static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        ClientId = b.ClientId,
        ClientName = b.Client?.ClientName ?? "Unknown",
        Date = b.ScheduledDate,
        Hour = b.ScheduledTimeSlot?.Hours ?? 0,
        Status = b.Status,
        ServicesCount = b.BookingServices?.Count ?? 0,
        CreatedAt = b.CreatedAt
    };

    private static BookingDetailDto MapToDetailDto(Booking b) => new()
    {
        Id = b.Id,
        ClientId = b.ClientId,
        Date = b.ScheduledDate,
        Hour = b.ScheduledTimeSlot?.Hours ?? 0,
        Status = b.Status,
        ServicesCount = b.BookingServices?.Count ?? 0,
        CreatedAt = b.CreatedAt,
        ClientName = b.Client?.ClientName ?? "",
        ClientPhone = b.Client?.Contacts?.FirstOrDefault()?.Phone ?? "",
        ClientEmail = b.Client?.Contacts?.FirstOrDefault()?.Email,
        AssignedEmployeeId = b.AssignedEmployeeId,
        AssignedEmployeeName = b.AssignedEmployee != null
            ? $"{b.AssignedEmployee.FirstName} {b.AssignedEmployee.LastName}"
            : null,
        Services = b.BookingServices.Select(bs => new BookingServiceDto
        {
            Id = bs.Id,
            ServiceCatalogId = bs.ServiceCatalogId,
            ServiceName = bs.ServiceCatalog?.Name ?? "",
            EstimatedPrice = bs.EstimatedPrice,
            FinalPrice = bs.FinalPrice,
            Quantity = bs.Quantity,
            Notes = bs.Notes
        }).ToList()
    };
}
