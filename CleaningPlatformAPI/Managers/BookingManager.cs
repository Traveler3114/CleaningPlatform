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
        var bookings = await _db.Bookings.Where(b => b.Date.Date == date.Date).ToListAsync();
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _db.Bookings.OrderByDescending(b => b.Date).ToListAsync();
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

        var booking = new Booking
        {
            CustomerName = dto.CustomerName,
            Phone = dto.Phone,
            Date = dto.Date.Date,
            Hour = dto.Hour,
            Status = BookingStatus.Reserved,
            CreatedAt = DateTime.UtcNow
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
        booking.Status = bookingStatus;
        await _db.SaveChangesAsync();
        return OperationResult<BookingDto>.Ok(MapToDto(booking));
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        CustomerName = b.CustomerName,
        Phone = b.Phone,
        Date = b.Date,
        Hour = b.Hour,
        Status = b.Status.ToString(),
        CreatedAt = b.CreatedAt
    };
}
