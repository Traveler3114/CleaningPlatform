namespace VechileCleaningAPI.Entities;

public class SlotOverride
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? Hour { get; set; }
    public bool IsClosed { get; set; }
    public int? Capacity { get; set; }
}
