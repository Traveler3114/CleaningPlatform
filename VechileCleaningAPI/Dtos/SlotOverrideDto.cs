namespace VechileCleaningAPI.Dtos;

public class SlotOverrideDto
{
    public DateTime Date { get; set; }
    public int? Hour { get; set; }
    public bool IsClosed { get; set; }
    public int? Capacity { get; set; }
}
