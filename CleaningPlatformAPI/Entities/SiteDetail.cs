using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entites
{
    [Table("SiteDetail")]
    public class SiteDetail
    {
        [Key]
        public int BookingId { get; set; }

        [Required, MaxLength(500)]
        public string ServiceAddress { get; set; }

        public string? Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }
    }
}