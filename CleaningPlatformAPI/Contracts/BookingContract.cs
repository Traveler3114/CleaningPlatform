using CleaningPlatformAPI.Enums;

namespace CleaningPlatformAPI.Contracts;

public class BookingResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public int ServicesCount { get; set; }
    public string? SiteName { get; set; }
    public List<AssignedEmployeeResponse> AssignedEmployees { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public int? RecurringScheduleId { get; set; }

    public string? ClientPhone { get; set; }
    public string? ClientEmail { get; set; }
    public List<BookingServiceResponse>? Services { get; set; }

    public string? LicensePlate { get; set; }
    public string? CarModel { get; set; }
    public string? BoatType { get; set; }
    public decimal? LengthMeters { get; set; }
}

public class AssignedEmployeeResponse
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int AssignmentId { get; set; }
}

public class BookingServiceResponse
{
    public int Id { get; set; }
    public int ServiceCatalogId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public record CreateBookingRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public int ServiceCatalogId { get; set; }
}

public record UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public record AddServiceRequest
{
    public int ServiceCatalogId { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}

public record UpdateServicePriceRequest
{
    public decimal? FinalPrice { get; set; }
}

public record AssignEmployeeRequest
{
    public int EmployeeId { get; set; }
}

public record CreateAdminBookingRequest
{
    public int ClientId { get; set; }
    public int? SiteId { get; set; }
    public BookingServiceType ServiceType { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public string? Notes { get; set; }
    public List<AddServiceRequest> Services { get; set; } = [];
}
