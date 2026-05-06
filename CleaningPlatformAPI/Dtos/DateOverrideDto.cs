namespace CleaningPlatformAPI.Dtos;

public class DateOverrideDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? StartHour { get; set; }
    public int? EndHour { get; set; }
    public int? Capacity { get; set; }
    public bool IsFullyClosed { get; set; }
}
