using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("BoatBookingDetails")]
    public class BoatBookingDetails
    {
        [Key]
        public int BookingId { get; set; }

        [Required, MaxLength(100)]
        public string BoatType { get; set; } = string.Empty;

        public decimal? LengthMeters { get; set; }
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
    }
}
