namespace VechileCleaningAPI.Entities;

public class WeeklySchedule
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public bool IsClosed { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int DefaultCapacity { get; set; }
}
