using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class AvailabilityManager
{
    private readonly AppDbContext _db;

    public AvailabilityManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AvailabilityResponse>> GetSlotsAsync(DateTime date, CancellationToken ct = default)
    {
        var dateOverride = await _db.DateOverrides.FirstOrDefaultAsync(o => o.Date.Date == date.Date, ct);
        if (dateOverride != null && dateOverride.IsFullyClosed)
            return [];

        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == (int)date.DayOfWeek, ct);
        if (schedule == null)
            return [];

        var startHour = dateOverride?.StartHour ?? schedule.StartHour;
        var endHour = dateOverride?.EndHour ?? schedule.EndHour;
        var defaultCapacity = dateOverride?.Capacity ?? schedule.Capacity;

        var bookings = await _db.Bookings
            .Where(b => b.ScheduledDate.Date == date.Date && b.Status != BookingStatus.Cancelled)
            .ToListAsync(ct);

        var slots = new List<AvailabilityResponse>();
        var isToday = date.Date == DateTime.UtcNow.Date;

        for (var h = startHour; h < endHour; h++)
        {
            if (isToday && h <= DateTime.UtcNow.Hour)
                continue;

            var booked = bookings.Count(b => b.ScheduledTimeSlot.HasValue && b.ScheduledTimeSlot.Value.Hours == h);
            slots.Add(ScheduleMapper.ToAvailabilityResponse(h, defaultCapacity, booked, defaultCapacity == 0));
        }

        return slots;
    }
}
