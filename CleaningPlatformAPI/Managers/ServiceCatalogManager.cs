using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class ServiceCatalogManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public ServiceCatalogManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<List<ServiceCatalogResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var services = await _db.ServiceCatalog
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return services.Select(ServiceCatalogMapper.ToResponse).ToList();
    }

    public async Task<OperationResult<ServiceCatalogResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ServiceCatalog.FindAsync([id], ct);
        if (entity is null)
            return OperationResult<ServiceCatalogResponse>.Fail("Service not found.");
        return OperationResult<ServiceCatalogResponse>.Ok(ServiceCatalogMapper.ToResponse(entity));
    }

    public async Task<OperationResult<ServiceCatalogResponse>> CreateAsync(ServiceCatalogUpsertRequest dto, CancellationToken ct = default)
    {
        var code = dto.CatalogCode.Trim();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return OperationResult<ServiceCatalogResponse>.Fail(_localizer["err_catalog_required"]);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat" };
        var serviceType = dto.ServiceType?.Trim();
        if (string.IsNullOrWhiteSpace(serviceType) || !validServiceTypes.Contains(serviceType))
            return OperationResult<ServiceCatalogResponse>.Fail("Service type must be one of: Vehicle, SiteBased, Boat.");

        var validCategories = new[] { "Stairs", "Office", "Private", "Special", "Carpet", "Furniture", "Exterior", "Laundry", "Vehicle", "Boat" };
        var category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        if (category is not null && !validCategories.Contains(category))
            return OperationResult<ServiceCatalogResponse>.Fail("Category must be one of: Stairs, Office, Private, Special, Carpet, Furniture, Exterior, Laundry, Vehicle, Boat.");

        var exists = await _db.ServiceCatalog.AnyAsync(s => s.CatalogCode == code, ct);
        if (exists)
            return OperationResult<ServiceCatalogResponse>.Fail(_localizer["err_catalog_code_exists"]);

        var now = DateTime.UtcNow;
        var entity = new ServiceCatalog
        {
            CatalogCode = code,
            Name = name,
            Category = category,
            Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim(),
            BasePrice = dto.BasePrice,
            ApproxTime = dto.ApproxTime,
            ServiceType = serviceType,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.ServiceCatalog.Add(entity);
        await _db.SaveChangesAsync(ct);

        return OperationResult<ServiceCatalogResponse>.Ok(ServiceCatalogMapper.ToResponse(entity));
    }

    public async Task<OperationResult<ServiceCatalogResponse>> UpdateAsync(int id, ServiceCatalogUpsertRequest dto, CancellationToken ct = default)
    {
        var entity = await _db.ServiceCatalog.FindAsync([id], ct);
        if (entity is null)
            return OperationResult<ServiceCatalogResponse>.Fail("Service not found.");

        var code = dto.CatalogCode.Trim();
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return OperationResult<ServiceCatalogResponse>.Fail(_localizer["err_catalog_required"]);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat" };
        var serviceType = dto.ServiceType?.Trim();
        if (string.IsNullOrWhiteSpace(serviceType) || !validServiceTypes.Contains(serviceType))
            return OperationResult<ServiceCatalogResponse>.Fail("Service type must be one of: Vehicle, SiteBased, Boat.");

        var validCategories = new[] { "Stairs", "Office", "Private", "Special", "Carpet", "Furniture", "Exterior", "Laundry", "Vehicle", "Boat" };
        var category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        if (category is not null && !validCategories.Contains(category))
            return OperationResult<ServiceCatalogResponse>.Fail("Category must be one of: Stairs, Office, Private, Special, Carpet, Furniture, Exterior, Laundry, Vehicle, Boat.");

        if (!string.Equals(entity.CatalogCode, code, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _db.ServiceCatalog.AnyAsync(s => s.CatalogCode == code && s.Id != id, ct);
            if (exists)
                return OperationResult<ServiceCatalogResponse>.Fail(_localizer["err_catalog_code_exists"]);
        }

        entity.CatalogCode = code;
        entity.Name = name;
        entity.Category = category;
        entity.Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim();
        entity.BasePrice = dto.BasePrice;
        entity.ApproxTime = dto.ApproxTime;
        entity.ServiceType = serviceType;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<ServiceCatalogResponse>.Ok(ServiceCatalogMapper.ToResponse(entity));
    }

    public async Task<OperationResult<string>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ServiceCatalog.FindAsync([id], ct);
        if (entity is null)
            return OperationResult<string>.Fail("Service not found.");

        _db.ServiceCatalog.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Service deleted.");
    }

    // ── Inventory Requirements ─────────────────────────────────

    public async Task<List<RequirementResponse>> GetRequirementsAsync(int serviceId, CancellationToken ct = default)
    {
        var requirements = await _db.ServiceInventoryRequirements
            .Include(r => r.Inventory)
            .Where(r => r.ServiceCatalogId == serviceId)
            .ToListAsync(ct);

        return requirements.Select(r => new RequirementResponse
        {
            ServiceCatalogId = r.ServiceCatalogId,
            InventoryId = r.InventoryId,
            InventoryName = r.Inventory?.Name ?? string.Empty,
            Unit = (r.Inventory?.Unit ?? default).ToString() ?? string.Empty,
            InventoryType = r.Inventory?.Type ?? string.Empty,
            QuantityNeeded = r.QuantityNeeded
        }).ToList();
    }

    public async Task<OperationResult<RequirementResponse>> AddRequirementAsync(int serviceId, RequirementUpsertRequest dto, CancellationToken ct = default)
    {
        var serviceExists = await _db.ServiceCatalog.AnyAsync(s => s.Id == serviceId, ct);
        if (!serviceExists)
            return OperationResult<RequirementResponse>.Fail("Service not found.");

        var inventoryExists = await _db.Inventory.AnyAsync(i => i.Id == dto.InventoryId, ct);
        if (!inventoryExists)
            return OperationResult<RequirementResponse>.Fail("Inventory item not found.");

        var alreadyExists = await _db.ServiceInventoryRequirements
            .AnyAsync(r => r.ServiceCatalogId == serviceId && r.InventoryId == dto.InventoryId, ct);
        if (alreadyExists)
            return OperationResult<RequirementResponse>.Fail("This inventory item is already linked to this service.");

        if (dto.QuantityNeeded <= 0)
            return OperationResult<RequirementResponse>.Fail("Quantity needed must be greater than zero.");

        var entity = new ServiceInventoryRequirement
        {
            ServiceCatalogId = serviceId,
            InventoryId = dto.InventoryId,
            QuantityNeeded = dto.QuantityNeeded
        };

        _db.ServiceInventoryRequirements.Add(entity);
        await _db.SaveChangesAsync(ct);

        var inventory = await _db.Inventory.FindAsync([dto.InventoryId], ct);
        return OperationResult<RequirementResponse>.Ok(new RequirementResponse
        {
            ServiceCatalogId = entity.ServiceCatalogId,
            InventoryId = entity.InventoryId,
            InventoryName = inventory?.Name ?? string.Empty,
            Unit = (inventory?.Unit ?? default).ToString() ?? string.Empty,
            InventoryType = inventory?.Type ?? string.Empty,
            QuantityNeeded = entity.QuantityNeeded
        });
    }

    public async Task<OperationResult<RequirementResponse>> UpdateRequirementAsync(int serviceId, int inventoryId, RequirementUpsertRequest dto, CancellationToken ct = default)
    {
        var entity = await _db.ServiceInventoryRequirements
            .Include(r => r.Inventory)
            .FirstOrDefaultAsync(r => r.InventoryId == inventoryId && r.ServiceCatalogId == serviceId, ct);
        if (entity is null)
            return OperationResult<RequirementResponse>.Fail("Requirement not found.");

        if (dto.QuantityNeeded <= 0)
            return OperationResult<RequirementResponse>.Fail("Quantity needed must be greater than zero.");

        entity.QuantityNeeded = dto.QuantityNeeded;
        await _db.SaveChangesAsync(ct);

        return OperationResult<RequirementResponse>.Ok(new RequirementResponse
        {
            ServiceCatalogId = entity.ServiceCatalogId,
            InventoryId = entity.InventoryId,
            InventoryName = entity.Inventory?.Name ?? string.Empty,
            Unit = (entity.Inventory?.Unit ?? default).ToString() ?? string.Empty,
            InventoryType = entity.Inventory?.Type ?? string.Empty,
            QuantityNeeded = entity.QuantityNeeded
        });
    }

    public async Task<OperationResult<string>> RemoveRequirementAsync(int serviceId, int inventoryId, CancellationToken ct = default)
    {
        var entity = await _db.ServiceInventoryRequirements
            .FirstOrDefaultAsync(r => r.InventoryId == inventoryId && r.ServiceCatalogId == serviceId, ct);
        if (entity is null)
            return OperationResult<string>.Fail("Requirement not found.");

        _db.ServiceInventoryRequirements.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Requirement removed.");
    }
}
