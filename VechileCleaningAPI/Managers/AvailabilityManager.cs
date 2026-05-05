using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Entities;

namespace VechileCleaningAPI.Managers;

public class AvailabilityManager
{
    private readonly AppDbContext _db;

    public AvailabilityManager(AppDbContext db)
    {
        _db = db;
    }

    // Inside AvailabilityManager.cs
    // AvailabilityManager.cs refactoring
    public async Task<List<AvailabilityDto>> GetSlotsAsync(DateTime date)
    {
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == (int)date.DayOfWeek);
        if (schedule == null || schedule.IsClosed)
            return new List<AvailabilityDto>();

        // Get daily overrides for this specific date
        var overrides = await _db.HourOverrides
            .Where(o => o.Date.Date == date.Date)
            .ToListAsync();

        var bookings = await _db.Bookings
            .Where(b => b.Date.Date == date.Date && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        var slots = new List<AvailabilityDto>();

        for (int h = schedule.StartHour; h < schedule.EndHour; h++)
        {
            // Find if there is a daily override for this specific hour
            var hourOverride = overrides.FirstOrDefault(o => o.Hour == h);

            // Logic: Use override capacity if it exists, otherwise default to 2
            int effectiveCapacity = hourOverride?.Capacity ?? 2; //GET READ OF THE MAGIC NUMBER 2 STORE DEFAULT IN DB

            int booked = bookings.Count(b => b.Hour == h);

            slots.Add(new AvailabilityDto
            {
                Hour = h,
                Capacity = effectiveCapacity,
                Booked = booked,
                Available = Math.Max(0, effectiveCapacity - booked)
            });
        }
        return slots;
    }
}
