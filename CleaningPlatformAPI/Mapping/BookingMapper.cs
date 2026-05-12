using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class BookingMapper
{
    public static BookingResponse ToResponse(Booking b) => new()
    {
        Id = b.Id,
        ClientId = b.ClientId,
        ClientName = b.Client?.ClientName ?? "Unknown",
        Date = b.ScheduledDate,
        Hour = b.ScheduledTimeSlot?.Hours ?? 0,
        Status = b.Status.ToString(),
        ServicesCount = b.BookingServices?.Count ?? 0,
        AssignedEmployees = b.Assignments?.Select(a => new AssignedEmployeeResponse
        {
            AssignmentId = a.Id,
            EmployeeId = a.EmployeeId,
            FullName = $"{a.Employee.FirstName} {a.Employee.LastName}".Trim(),
            Role = a.Employee.Role?.Name ?? string.Empty
        }).ToList() ?? new(),
        CreatedAt = b.CreatedAt
    };

    public static BookingResponse ToDetailResponse(Booking b)
    {
        var primaryContact = b.Client?.Contacts?
            .FirstOrDefault(c => c.IsPrimary && c.IsActive)
            ?? b.Client?.Contacts?.FirstOrDefault(c => c.IsActive);

        var response = ToResponse(b);
        response.ClientPhone = primaryContact?.Phone;
        response.ClientEmail = primaryContact?.Email;
        response.Services = b.BookingServices.Select(bs => new BookingServiceResponse
        {
            Id = bs.Id,
            ServiceCatalogId = bs.ServiceCatalogId,
            ServiceName = bs.ServiceCatalog?.Name ?? string.Empty,
            EstimatedPrice = bs.EstimatedPrice,
            FinalPrice = bs.FinalPrice,
            Quantity = bs.Quantity,
            Notes = bs.Notes
        }).ToList();
        return response;
    }
}
