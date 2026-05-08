using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class ServiceCatalogManager
{
    private readonly AppDbContext _db;

    public ServiceCatalogManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceCatalogDto>> GetAllAsync()
    {
        var services = await _db.ServiceCatalog
            .OrderBy(s => s.Name)
            .ToListAsync();

        return services.Select(MapToDto).ToList();
    }

    public async Task<OperationResult<ServiceCatalogDto>> CreateAsync(ServiceCatalogUpsertDto dto)
    {
        var code = dto.CatalogCode.Trim();
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return OperationResult<ServiceCatalogDto>.Fail("Catalog code and name are required.");

        var exists = await _db.ServiceCatalog.AnyAsync(s => s.CatalogCode == code);
        if (exists)
            return OperationResult<ServiceCatalogDto>.Fail("Catalog code already exists.");

        var now = DateTime.UtcNow;
        var entity = new ServiceCatalog
        {
            CatalogCode = code,
            Name = name,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim(),
            Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim(),
            PriceMin = dto.PriceMin,
            PriceMax = dto.PriceMax,
            PriceAvg = dto.PriceAvg,
            DefaultMarginPct = dto.DefaultMarginPct,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.ServiceCatalog.Add(entity);
        await _db.SaveChangesAsync();

        return OperationResult<ServiceCatalogDto>.Ok(MapToDto(entity));
    }

    public async Task<OperationResult<ServiceCatalogDto>> UpdateAsync(int id, ServiceCatalogUpsertDto dto)
    {
        var entity = await _db.ServiceCatalog.FindAsync(id);
        if (entity == null)
            return OperationResult<ServiceCatalogDto>.Fail("Service not found.");

        var code = dto.CatalogCode.Trim();
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return OperationResult<ServiceCatalogDto>.Fail("Catalog code and name are required.");

        if (!string.Equals(entity.CatalogCode, code, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _db.ServiceCatalog.AnyAsync(s => s.CatalogCode == code && s.Id != id);
            if (exists)
                return OperationResult<ServiceCatalogDto>.Fail("Catalog code already exists.");
        }

        entity.CatalogCode = code;
        entity.Name = name;
        entity.Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category.Trim();
        entity.Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim();
        entity.PriceMin = dto.PriceMin;
        entity.PriceMax = dto.PriceMax;
        entity.PriceAvg = dto.PriceAvg;
        entity.DefaultMarginPct = dto.DefaultMarginPct;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<ServiceCatalogDto>.Ok(MapToDto(entity));
    }

    public async Task<OperationResult<string>> DeleteAsync(int id)
    {
        var entity = await _db.ServiceCatalog.FindAsync(id);
        if (entity == null)
            return OperationResult<string>.Fail("Service not found.");

        _db.ServiceCatalog.Remove(entity);
        await _db.SaveChangesAsync();
        return OperationResult<string>.Ok("Service deleted.");
    }

    private static ServiceCatalogDto MapToDto(ServiceCatalog s) => new()
    {
        Id = s.Id,
        CatalogCode = s.CatalogCode,
        Name = s.Name,
        Category = s.Category,
        Unit = s.Unit,
        PriceMin = s.PriceMin,
        PriceMax = s.PriceMax,
        PriceAvg = s.PriceAvg,
        DefaultMarginPct = s.DefaultMarginPct,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
