using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class BookingView
{
    public int BookingId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public string? PrimaryContact { get; set; }
    public string? ContactPhone { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public TimeSpan? ScheduledTimeSlot { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? SiteName { get; set; }
    public string? SiteAddress { get; set; }
    public string? SiteCity { get; set; }
    public string? SiteType { get; set; }
    public decimal? FloorAreaM2 { get; set; }
    public decimal? EstimatedTotal { get; set; }
    public decimal? FinalTotal { get; set; }
    public string? AssignedEmployee { get; set; }
    public int ServiceCount { get; set; }
    public string? ServiceItems { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? InvoiceStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? LicensePlate { get; set; }
    public string? CarModel { get; set; }
    public string? BoatType { get; set; }
    public decimal? LengthMeters { get; set; }
}
