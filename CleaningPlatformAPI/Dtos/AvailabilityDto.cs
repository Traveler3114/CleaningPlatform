namespace CleaningPlatformAPI.Dtos;

public class AvailabilityDto
{
    public int Hour { get; set; }
    public int Capacity { get; set; }
    public int Booked { get; set; }
    public int Available { get; set; }
    public bool IsClosed { get; set; }
}
