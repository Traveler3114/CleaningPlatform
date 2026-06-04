using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class InventoryManager
{
    private readonly AppDbContext _db;

    public InventoryManager(AppDbContext db) { _db = db; }

    public async Task<List<InventoryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _db.Inventory
            .OrderBy(i => i.Name)
            .ToListAsync(ct);
        return items.Select(InventoryMapper.ToResponse).ToList();
    }

    public async Task<OperationResult<InventoryResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _db.Inventory.FindAsync([id], ct);
        if (item is null)
            return OperationResult<InventoryResponse>.Fail("Inventory item not found.");
        return OperationResult<InventoryResponse>.Ok(InventoryMapper.ToResponse(item));
    }

    public async Task<OperationResult<InventoryResponse>> CreateAsync(InventoryUpsertRequest dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<InventoryResponse>.Fail("Item name is required.");
        if (!Enum.IsDefined(typeof(InventoryUnit), dto.Unit))
            return OperationResult<InventoryResponse>.Fail("Invalid unit value.");
        if (dto.Quantity < 0)
            return OperationResult<InventoryResponse>.Fail("Quantity cannot be negative.");

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

        return OperationResult<InventoryResponse>.Ok(InventoryMapper.ToResponse(entity));
    }

    public async Task<OperationResult<InventoryResponse>> UpdateAsync(int id, InventoryUpsertRequest dto, CancellationToken ct = default)
    {
        var entity = await _db.Inventory.FindAsync([id], ct);
        if (entity is null)
            return OperationResult<InventoryResponse>.Fail("Inventory item not found.");

        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<InventoryResponse>.Fail("Item name is required.");
        if (!Enum.IsDefined(typeof(InventoryUnit), dto.Unit))
            return OperationResult<InventoryResponse>.Fail("Invalid unit value.");
        if (dto.Quantity < 0)
            return OperationResult<InventoryResponse>.Fail("Quantity cannot be negative.");

        entity.Name = name;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        entity.Type = dto.Type ?? "Consumable";
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<InventoryResponse>.Ok(InventoryMapper.ToResponse(entity));
    }

    public async Task<OperationResult<string>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Inventory.FindAsync([id], ct);
        if (entity is null)
            return OperationResult<string>.Fail("Inventory item not found.");

        var hasReferences = await _db.ServiceInventoryRequirements.AnyAsync(r => r.InventoryId == id, ct);
        if (hasReferences)
            return OperationResult<string>.Fail("Cannot delete this item because it is referenced by one or more service requirements.");

        _db.Inventory.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Inventory item deleted.");
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
