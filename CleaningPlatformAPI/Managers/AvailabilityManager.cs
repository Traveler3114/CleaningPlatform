using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class AvailabilityManager
{
    private readonly AppDbContext _db;

    public AvailabilityManager(AppDbContext db) { _db = db; }

    public async Task<List<AvailabilityResponse>> GetSlotsAsync(DateTime date, CancellationToken ct = default)
    {
        var dateOverride = await _db.DateOverrides.FirstOrDefaultAsync(o => o.Date == DateOnly.FromDateTime(date.Date), ct);
        if (dateOverride is not null && dateOverride.IsFullyClosed)
            return [];

        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == (int)date.DayOfWeek, ct);
        if (schedule is null)
            return [];

        var startHour = dateOverride?.StartHour ?? schedule.StartHour;
        var endHour = dateOverride?.EndHour ?? schedule.EndHour;
        var defaultCapacity = dateOverride?.Capacity ?? schedule.Capacity;

        if (startHour == endHour)
            return [ScheduleMapper.ToAvailabilityResponse(startHour, 0, 0, true)];

        var bookedCounts = await _db.Bookings
            .Where(b => b.ScheduledDate.Date == date.Date && b.Status != BookingStatus.Cancelled && b.ScheduledTimeSlot != null)
            .GroupBy(b => b.ScheduledTimeSlot!.Value.Hours)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Hour, g => g.Count, ct);

        List<AvailabilityResponse> slots = [];
        TimeZoneInfo croatiaZone;
        try
        {
            croatiaZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            croatiaZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zagreb");
        }
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, croatiaZone);
        var isToday = date.Date == now.Date;

        for (var h = startHour; h < endHour; h++)
        {
            if (isToday && h <= now.Hour)
                continue;

            var booked = bookedCounts.GetValueOrDefault(h, 0);
            slots.Add(ScheduleMapper.ToAvailabilityResponse(h, defaultCapacity, booked, defaultCapacity == 0));
        }

        return slots;
    }
}

