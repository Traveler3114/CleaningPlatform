namespace CleaningPlatformAPI.Dtos;

public class ServiceCatalogUpsertDto
{
    public string CatalogCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultPrice { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public decimal? PriceAvg { get; set; }
    public decimal? DefaultMarginPct { get; set; }
    public bool IsActive { get; set; } = true;
}
