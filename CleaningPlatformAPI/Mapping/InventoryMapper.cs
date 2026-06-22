using System.Linq.Expressions;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class InventoryMapper
{
    public static Expression<Func<Inventory, InventoryResponse>> Projection => i => new()
    {
        Id = i.Id,
        Name = i.Name,
        Quantity = i.Quantity,
        Unit = i.Unit,
        Category = i.Category,
        Type = i.Type,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };

    public static InventoryResponse ToResponse(Inventory i) => new()
    {
        Id = i.Id,
        Name = i.Name,
        Quantity = i.Quantity,
        Unit = i.Unit,
        Category = i.Category,
        Type = i.Type,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };
}
