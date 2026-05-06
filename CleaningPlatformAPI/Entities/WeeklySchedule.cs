namespace CleaningPlatformAPI.Entities;

public class WeeklySchedule
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int Capacity { get; set; }
}
