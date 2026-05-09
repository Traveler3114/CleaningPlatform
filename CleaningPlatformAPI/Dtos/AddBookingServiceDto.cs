namespace CleaningPlatformAPI.Dtos;

public class AddBookingServiceDto
{
    public int ServiceCatalogId { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}
