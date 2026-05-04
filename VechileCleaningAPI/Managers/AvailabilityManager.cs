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
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == (int)date.DayOfWeek);
        if (schedule == null || schedule.IsClosed)
            return new List<AvailabilityDto>();

        var overrides = await _db.SlotOverrides.Where(o => o.Date.Date == date.Date).ToListAsync();
        var dayOverride = overrides.FirstOrDefault(o => o.Hour == null);

        if (dayOverride != null && dayOverride.IsClosed)
            return new List<AvailabilityDto>();

        var bookings = await _db.Bookings
            .Where(b => b.Date.Date == date.Date && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        var slots = new List<AvailabilityDto>();
        var startHour = schedule.StartHour;
        var endHour = schedule.EndHour;
        var defaultCapacity = dayOverride?.Capacity ?? schedule.DefaultCapacity;

        for (int h = startHour; h < endHour; h++)
        {
            var hourOverride = overrides.FirstOrDefault(o => o.Hour == h);
            bool isClosed = hourOverride?.IsClosed ?? false;
            int capacity = hourOverride?.Capacity ?? defaultCapacity;
            int booked = bookings.Count(b => b.Hour == h);
            slots.Add(new AvailabilityDto
            {
                Hour = h,
                Capacity = capacity,
                Booked = booked,
                Available = Math.Max(0, capacity - booked),
                IsClosed = isClosed
            });
        }
        return slots;
    }
}
