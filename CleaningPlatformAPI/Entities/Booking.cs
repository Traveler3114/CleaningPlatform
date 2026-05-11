using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public int? SiteId { get; set; }

        [Required, MaxLength(50)]
        public string ServiceType { get; set; }   // Vehicle, SiteBased, Boat

        public DateTime ScheduledDate { get; set; }
        public TimeSpan? ScheduledTimeSlot { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }   // Stored as string: Pending, Confirmed, InProgress, Completed, Cancelled

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; }

        [ForeignKey(nameof(SiteId))]
        public Site? Site { get; set; }

        public VehicleBookingDetails? VehicleDetails { get; set; }
        public SiteDetail? SiteDetail { get; set; }
        public BoatBookingDetails? BoatDetails { get; set; }

        public ICollection<BookingAssignment> Assignments { get; set; } = new List<BookingAssignment>();
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<InvoiceBooking> InvoiceBookings { get; set; } = new List<InvoiceBooking>();
    }
}
