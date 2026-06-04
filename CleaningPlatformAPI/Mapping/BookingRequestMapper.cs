using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class BookingRequestMapper
{
    public static BookingRequestResponse ToResponse(BookingRequest r) => new()
    {
        Id = r.Id,
        ContactName = r.ContactName,
        Phone = r.Phone,
        Email = r.Email,
        Notes = r.Notes,
        EstimatedPrice = r.EstimatedPrice,
        AdminNotes = r.AdminNotes,
        Status = r.Status.ToString(),
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Services = r.RequestServices?.Select(s => new BookingRequestServiceResponse
        {
            ServiceCatalogId = s.ServiceCatalogId,
            ServiceName = s.ServiceCatalog?.Name ?? string.Empty
        }).ToList() ?? []
    };

    public static CustomerPreviewResponse ToPreviewResponse(BookingRequest r) => new()
    {
        Id = r.Id,
        ContactName = r.ContactName,
        Phone = r.Phone,
        Email = r.Email,
        Notes = r.Notes,
        EstimatedPrice = r.EstimatedPrice,
        AdminNotes = r.AdminNotes,
        Status = r.Status.ToString(),
        Services = r.RequestServices?.Select(s => new BookingRequestServiceResponse
        {
            ServiceCatalogId = s.ServiceCatalogId,
            ServiceName = s.ServiceCatalog?.Name ?? string.Empty
        }).ToList() ?? []
    };
}
