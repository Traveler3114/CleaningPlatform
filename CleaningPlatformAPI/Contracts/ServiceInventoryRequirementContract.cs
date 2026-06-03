namespace CleaningPlatformAPI.Contracts;

public class RequirementResponse
{
    public int Id { get; set; }
    public int ServiceCatalogId { get; set; }
    public int InventoryId { get; set; }
    public string InventoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string InventoryType { get; set; } = string.Empty;
    public decimal QuantityNeeded { get; set; }
}

public record RequirementUpsertRequest
{
    public int InventoryId { get; set; }
    public decimal QuantityNeeded { get; set; }
}
