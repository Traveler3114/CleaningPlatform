using System;
using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities
{
    [Keyless]
    public class BookingView
    {
        public int BookingId { get; set; }
        public string ClientName { get; set; }
        public string ClientType { get; set; }
        public string? PrimaryContact { get; set; }
        public string? ContactPhone { get; set; }
        public string ServiceType { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan? ScheduledTimeSlot { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public decimal? EstimatedTotal { get; set; }
        public decimal? FinalTotal { get; set; }
        public string AssignedEmployee { get; set; }
        public int ServiceCount { get; set; }
        public string? ServiceItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Service-specific fields
        public string? LicensePlate { get; set; }
        public string? CarModel { get; set; }
        public string? ServiceAddress { get; set; }
        public string? BoatType { get; set; }
        public decimal? LengthMeters { get; set; }
    }
}
