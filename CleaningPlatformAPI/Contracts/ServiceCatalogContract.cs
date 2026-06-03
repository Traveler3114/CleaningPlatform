namespace CleaningPlatformAPI.Contracts;

public class ServiceCatalogResponse
{
    public int Id { get; set; }
    public string CatalogCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public decimal? BasePrice { get; set; }
    public int? ApproxTime { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record ServiceCatalogUpsertRequest
{
    public string CatalogCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public decimal? BasePrice { get; set; }
    public int? ApproxTime { get; set; }
    public string ServiceType { get; set; } = "Vehicle";
    public bool IsActive { get; set; } = true;
}
