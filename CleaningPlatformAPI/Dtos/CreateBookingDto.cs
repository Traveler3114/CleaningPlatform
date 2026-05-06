namespace CleaningPlatformAPI.Dtos;

public class CreateBookingDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hour { get; set; }
}
