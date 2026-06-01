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
}
