namespace CleaningPlatformAPI.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ServicesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
