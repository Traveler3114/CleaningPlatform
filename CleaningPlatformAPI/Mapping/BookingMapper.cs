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
        ServiceType = b.ServiceType.ToString(),
        Status = b.Status.ToString(),
        ServicesCount = b.BookingServices?.Count ?? 0,
        SiteName = b.Site?.SiteName,
        RecurringScheduleId = b.RecurringScheduleId,
        AssignedEmployees = b.Assignments?.Select(a => new AssignedEmployeeResponse
        {
            AssignmentId = a.Id,
            EmployeeId = a.EmployeeId,
            FullName = $"{a.Employee.FirstName} {a.Employee.LastName}".Trim(),
            Role = a.Employee.Role?.Name ?? string.Empty
        }).ToList() ?? new(),
        CreatedAt = b.CreatedAt
    };

    public static BookingResponse ToResponse(BookingView view, int clientId) => new()
    {
        Id = view.BookingId,
        ClientId = clientId,
        ClientName = view.ClientName,
        Date = view.ScheduledDate,
        Hour = view.ScheduledTimeSlot?.Hours ?? 0,
        ServiceType = string.Empty,
        Status = view.Status,
        ServicesCount = view.ServiceCount,
        AssignedEmployees = SplitAssignedEmployees(view.AssignedEmployee),
        CreatedAt = view.CreatedAt
    };

    private static List<AssignedEmployeeResponse> SplitAssignedEmployees(string? assignedEmployee)
    {
        if (string.IsNullOrWhiteSpace(assignedEmployee))
            return new();

        return assignedEmployee
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => new AssignedEmployeeResponse { FullName = name })
            .ToList();
    }

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
