using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

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

    public async Task<ServiceCatalogResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ServiceCatalog.FindAsync([id], ct);
        if (entity is null)
            throw new AppException("SERVICE_NOT_FOUND", "Service not found.", 404);
        return ServiceCatalogMapper.ToResponse(entity);
    }

    public async Task<ServiceCatalogResponse> CreateAsync(ServiceCatalogUpsertRequest dto, CancellationToken ct = default)
    {
        var code = dto.CatalogCode.Trim();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new AppException("CATALOG_REQUIRED", _localizer["err_catalog_required"], 422);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat" };
        var serviceType = dto.ServiceType?.Trim();
        if (string.IsNullOrWhiteSpace(serviceType) || !validServiceTypes.Contains(serviceType))
            throw new AppException("INVALID_SERVICE_TYPE", "Service type must be one of: Vehicle, SiteBased, Boat.", 422);

        var validCategories = new[] { "Stairs", "Office", "Private", "Special", "Carpet", "Furniture", "Exterior", "Laundry", "Vehicle", "Boat" };
        var category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        if (category is not null && !validCategories.Contains(category))
            throw new AppException("INVALID_CATEGORY", "Category must be one of: Stairs, Office, Private, Special, Carpet, Furniture, Exterior, Laundry, Vehicle, Boat.", 422);

        var exists = await _db.ServiceCatalog.AnyAsync(s => s.CatalogCode == code, ct);
        if (exists)
            throw new AppException("CATALOG_CODE_EXISTS", _localizer["err_catalog_code_exists"], 409);

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

        return ServiceCatalogMapper.ToResponse(entity);
    }

    public async Task<ServiceCatalogResponse> UpdateAsync(int id, ServiceCatalogUpsertRequest dto, CancellationToken ct = default)
    {
        var entity = await _db.ServiceCatalog.FindAsync([id], ct);
        if (entity is null)
            throw new AppException("SERVICE_NOT_FOUND", "Service not found.", 404);

        var code = dto.CatalogCode.Trim();
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new AppException("CATALOG_REQUIRED", _localizer["err_catalog_required"], 422);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat" };
        var serviceType = dto.ServiceType?.Trim();
        if (string.IsNullOrWhiteSpace(serviceType) || !validServiceTypes.Contains(serviceType))
            throw new AppException("INVALID_SERVICE_TYPE", "Service type must be one of: Vehicle, SiteBased, Boat.", 422);

        var validCategories = new[] { "Stairs", "Office", "Private", "Special", "Carpet", "Furniture", "Exterior", "Laundry", "Vehicle", "Boat" };
        var category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        if (category is not null && !validCategories.Contains(category))
            throw new AppException("INVALID_CATEGORY", "Category must be one of: Stairs, Office, Private, Special, Carpet, Furniture, Exterior, Laundry, Vehicle, Boat.", 422);

        if (!string.Equals(entity.CatalogCode, code, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _db.ServiceCatalog.AnyAsync(s => s.CatalogCode == code && s.Id != id, ct);
            if (exists)
                throw new AppException("CATALOG_CODE_EXISTS", _localizer["err_catalog_code_exists"], 409);
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
        return ServiceCatalogMapper.ToResponse(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ServiceCatalog.FindAsync([id], ct);
        if (entity is null)
            throw new AppException("SERVICE_NOT_FOUND", "Service not found.", 404);

        _db.ServiceCatalog.Remove(entity);
        await _db.SaveChangesAsync(ct);
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

    public async Task<RequirementResponse> AddRequirementAsync(int serviceId, RequirementUpsertRequest dto, CancellationToken ct = default)
    {
        var serviceExists = await _db.ServiceCatalog.AnyAsync(s => s.Id == serviceId, ct);
        if (!serviceExists)
            throw new AppException("SERVICE_NOT_FOUND", "Service not found.", 404);

        var inventoryExists = await _db.Inventory.AnyAsync(i => i.Id == dto.InventoryId, ct);
        if (!inventoryExists)
            throw new AppException("INVENTORY_NOT_FOUND", "Inventory item not found.", 404);

        var alreadyExists = await _db.ServiceInventoryRequirements
            .AnyAsync(r => r.ServiceCatalogId == serviceId && r.InventoryId == dto.InventoryId, ct);
        if (alreadyExists)
            throw new AppException("INVENTORY_ALREADY_LINKED", "This inventory item is already linked to this service.", 409);

        if (dto.QuantityNeeded <= 0)
            throw new AppException("QUANTITY_NEEDED_POSITIVE", "Quantity needed must be greater than zero.", 422);

        var entity = new ServiceInventoryRequirement
        {
            ServiceCatalogId = serviceId,
            InventoryId = dto.InventoryId,
            QuantityNeeded = dto.QuantityNeeded
        };

        _db.ServiceInventoryRequirements.Add(entity);
        await _db.SaveChangesAsync(ct);

        var inventory = await _db.Inventory.FindAsync([dto.InventoryId], ct);
        return new RequirementResponse
        {
            ServiceCatalogId = entity.ServiceCatalogId,
            InventoryId = entity.InventoryId,
            InventoryName = inventory?.Name ?? string.Empty,
            Unit = (inventory?.Unit ?? default).ToString() ?? string.Empty,
            InventoryType = inventory?.Type ?? string.Empty,
            QuantityNeeded = entity.QuantityNeeded
        };
    }

    public async Task<RequirementResponse> UpdateRequirementAsync(int serviceId, int inventoryId, RequirementUpsertRequest dto, CancellationToken ct = default)
    {
        var entity = await _db.ServiceInventoryRequirements
            .Include(r => r.Inventory)
            .FirstOrDefaultAsync(r => r.InventoryId == inventoryId && r.ServiceCatalogId == serviceId, ct);
        if (entity is null)
            throw new AppException("REQUIREMENT_NOT_FOUND", "Requirement not found.", 404);

        if (dto.QuantityNeeded <= 0)
            throw new AppException("QUANTITY_NEEDED_POSITIVE", "Quantity needed must be greater than zero.", 422);

        entity.QuantityNeeded = dto.QuantityNeeded;
        await _db.SaveChangesAsync(ct);

        return new RequirementResponse
        {
            ServiceCatalogId = entity.ServiceCatalogId,
            InventoryId = entity.InventoryId,
            InventoryName = entity.Inventory?.Name ?? string.Empty,
            Unit = (entity.Inventory?.Unit ?? default).ToString() ?? string.Empty,
            InventoryType = entity.Inventory?.Type ?? string.Empty,
            QuantityNeeded = entity.QuantityNeeded
        };
    }

    public async Task RemoveRequirementAsync(int serviceId, int inventoryId, CancellationToken ct = default)
    {
        var entity = await _db.ServiceInventoryRequirements
            .FirstOrDefaultAsync(r => r.InventoryId == inventoryId && r.ServiceCatalogId == serviceId, ct);
        if (entity is null)
            throw new AppException("REQUIREMENT_NOT_FOUND", "Requirement not found.", 404);

        _db.ServiceInventoryRequirements.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
