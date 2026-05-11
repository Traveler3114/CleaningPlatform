namespace CleaningPlatformAPI.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ServicesCount { get; set; }
    public List<AssignedEmployeeDto> AssignedEmployees { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
