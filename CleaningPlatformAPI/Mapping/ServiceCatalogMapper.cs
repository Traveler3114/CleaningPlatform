using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class ServiceCatalogMapper
{
    public static ServiceCatalogResponse ToResponse(ServiceCatalog s) => new()
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
        ServiceType = s.ServiceType,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
