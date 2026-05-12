namespace CleaningPlatformAPI.Contracts;

public class WeeklyScheduleResponse
{
    public int DayOfWeek { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int Capacity { get; set; }
}

public class DateOverrideResponse
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? StartHour { get; set; }
    public int? EndHour { get; set; }
    public int? Capacity { get; set; }
    public bool IsFullyClosed { get; set; }
}

public class AvailabilityResponse
{
    public int Hour { get; set; }
    public int Capacity { get; set; }
    public int Booked { get; set; }
    public int Available { get; set; }
    public bool IsClosed { get; set; }
}

public record WeeklyScheduleRequest
{
    public int DayOfWeek { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int Capacity { get; set; }
}

public record UpdateWeeklyScheduleRequest
{
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int Capacity { get; set; }
}

public record DateOverrideRequest
{
    public DateTime Date { get; set; }
    public int? StartHour { get; set; }
    public int? EndHour { get; set; }
    public int? Capacity { get; set; }
    public bool IsFullyClosed { get; set; }
}
