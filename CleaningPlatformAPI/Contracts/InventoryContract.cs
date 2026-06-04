using CleaningPlatformAPI.Enums;

namespace CleaningPlatformAPI.Contracts;

public class InventoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public InventoryUnit Unit { get; set; }
    public string? Category { get; set; }
    public string Type { get; set; } = "Consumable";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record InventoryUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public InventoryUnit Unit { get; set; } = InventoryUnit.Piece;
    public string? Category { get; set; }
    public string Type { get; set; } = "Consumable";
}

public record InventoryAdjustmentRequest
{
    public decimal AdjustmentAmount { get; set; }
}
