using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class InventoryManager
{
    private readonly AppDbContext _db;

    public InventoryManager(AppDbContext db) { _db = db; }

    public async Task<List<InventoryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Inventory
            .OrderBy(i => i.Name)
            .Select(InventoryMapper.Projection)
            .ToListAsync(ct);
    }

    public async Task<InventoryResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _db.Inventory.FindAsync([id], ct);
        if (item is null)
            throw new AppException("INVENTORY_NOT_FOUND", "Inventory item not found.", 404);
        return InventoryMapper.ToResponse(item);
    }

    public async Task<InventoryResponse> CreateAsync(InventoryUpsertRequest dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("ITEM_NAME_REQUIRED", "Item name is required.", 422);
        if (!Enum.IsDefined(typeof(InventoryUnit), dto.Unit))
            throw new AppException("INVALID_UNIT_VALUE", "Invalid unit value.", 422);
        if (dto.Quantity < 0)
            throw new AppException("NEGATIVE_QUANTITY", "Quantity cannot be negative.", 422);

        var now = DateTime.UtcNow;
        var entity = new Inventory
        {
            Name = name,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim(),
            Type = dto.Type ?? "Consumable",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Inventory.Add(entity);
        await _db.SaveChangesAsync(ct);

        return InventoryMapper.ToResponse(entity);
    }

    public async Task<InventoryResponse> UpdateAsync(int id, InventoryUpsertRequest dto, CancellationToken ct = default)
    {
        var entity = await _db.Inventory.FindAsync([id], ct);
        if (entity is null)
            throw new AppException("INVENTORY_NOT_FOUND", "Inventory item not found.", 404);

        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("ITEM_NAME_REQUIRED", "Item name is required.", 422);
        if (!Enum.IsDefined(typeof(InventoryUnit), dto.Unit))
            throw new AppException("INVALID_UNIT_VALUE", "Invalid unit value.", 422);
        if (dto.Quantity < 0)
            throw new AppException("NEGATIVE_QUANTITY", "Quantity cannot be negative.", 422);

        entity.Name = name;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        entity.Type = dto.Type ?? "Consumable";
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return InventoryMapper.ToResponse(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Inventory.FindAsync([id], ct);
        if (entity is null)
            throw new AppException("INVENTORY_NOT_FOUND", "Inventory item not found.", 404);

        var hasReferences = await _db.ServiceInventoryRequirements.AnyAsync(r => r.InventoryId == id, ct);
        if (hasReferences)
            throw new AppException("INVENTORY_IN_USE", "Cannot delete this item because it is referenced by one or more service requirements.", 409);

        _db.Inventory.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AdjustStockAsync(int inventoryId, decimal delta, CancellationToken ct = default)
    {
        var item = await _db.Inventory.FindAsync([inventoryId], ct);
        if (item is null || item.Type != "Consumable") return;
        item.Quantity = item.Quantity + delta;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
