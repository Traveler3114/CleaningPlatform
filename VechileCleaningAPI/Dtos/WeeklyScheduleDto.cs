namespace VechileCleaningAPI.Dtos;

public class WeeklyScheduleDto
{
    public int DayOfWeek { get; set; }
    public bool IsClosed { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public int DefaultCapacity { get; set; }
}
