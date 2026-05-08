namespace CleaningPlatformAPI.Dtos;

public class ServiceCatalogDto
{
    public int Id { get; set; }
    public string CatalogCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public decimal? PriceAvg { get; set; }
    public decimal? DefaultMarginPct { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
