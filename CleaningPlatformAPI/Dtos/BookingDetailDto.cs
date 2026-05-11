// CleaningPlatformAPI/Dtos/BookingDetailDto.cs

namespace CleaningPlatformAPI.Dtos;

public class BookingDetailDto : BookingDto
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }

    public List<AssignedEmployeeDto> AssignedEmployees { get; set; } = new();

    public List<BookingServiceDto> Services { get; set; } = new();
}

public class BookingServiceDto
{
    public int Id { get; set; }
    public int ServiceCatalogId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}
