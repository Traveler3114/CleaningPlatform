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

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        CustomerName = b.Client?.ClientName ?? "Unknown",
        Phone = b.Client?.Contacts?.FirstOrDefault(c => c.IsPrimary)?.Phone
            ?? b.Client?.Contacts?.FirstOrDefault()?.Phone
            ?? string.Empty,
        Date = b.ScheduledDate,
        Hour = b.ScheduledTimeSlot.HasValue
            ? (int)Math.Round(b.ScheduledTimeSlot.Value.TotalHours, MidpointRounding.AwayFromZero)
            : 0,
        Status = b.Status,
        CreatedAt = b.CreatedAt
    };
}
