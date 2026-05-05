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

    public async Task<List<AvailabilityDto>> GetSlotsAsync(DateTime date)
    {
        // Check DateOverride first
        var dateOverride = await _db.DateOverrides.FirstOrDefaultAsync(o => o.Date.Date == date.Date);
        if (dateOverride != null && dateOverride.IsFullyClosed)
            return new List<AvailabilityDto>();

        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == (int)date.DayOfWeek);
        if (schedule == null)
            return new List<AvailabilityDto>();

        // Use override hours if provided, otherwise fall back to schedule
        int startHour = dateOverride?.StartHour ?? schedule.StartHour;
        int endHour = dateOverride?.EndHour ?? schedule.EndHour;
        int defaultCapacity = dateOverride?.Capacity ?? schedule.Capacity;

        var bookings = await _db.Bookings
            .Where(b => b.Date.Date == date.Date && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        var slots = new List<AvailabilityDto>();

        for (int h = startHour; h < endHour; h++)
        {
            int booked = bookings.Count(b => b.Hour == h);

            slots.Add(new AvailabilityDto
            {
                Hour = h,
                Capacity = defaultCapacity,
                Booked = booked,
                Available = Math.Max(0, defaultCapacity - booked)
            });
        }
        return slots;
    }
}
