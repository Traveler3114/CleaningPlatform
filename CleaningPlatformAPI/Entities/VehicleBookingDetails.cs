using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("VehicleBookingDetails")]
    public class VehicleBookingDetails
    {
        [Key]
        public int BookingId { get; set; }

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; }

        [MaxLength(100)]
        public string? CarModel { get; set; }

        public string? Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }
    }
}
