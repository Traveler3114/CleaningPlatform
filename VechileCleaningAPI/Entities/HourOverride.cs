namespace VechileCleaningAPI.Entities;

public class HourOverride
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? Hour { get; set; }
    public int? Capacity { get; set; }
}
