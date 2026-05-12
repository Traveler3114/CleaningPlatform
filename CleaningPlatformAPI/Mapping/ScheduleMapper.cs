using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class ScheduleMapper
{
    public static WeeklyScheduleResponse ToWeeklyResponse(WeeklySchedule s) => new()
    {
        DayOfWeek = s.DayOfWeek,
        StartHour = s.StartHour,
        EndHour = s.EndHour,
        Capacity = s.Capacity,
    };

    public static DateOverrideResponse ToDateOverrideResponse(DateOverride o) => new()
    {
        Id = o.Id,
        Date = o.Date,
        StartHour = o.StartHour,
        EndHour = o.EndHour,
        Capacity = o.Capacity,
        IsFullyClosed = o.IsFullyClosed,
    };

    public static AvailabilityResponse ToAvailabilityResponse(int hour, int capacity, int booked, bool isClosed) => new()
    {
        Hour = hour,
        Capacity = capacity,
        Booked = booked,
        Available = Math.Max(0, capacity - booked),
        IsClosed = isClosed
    };
}
