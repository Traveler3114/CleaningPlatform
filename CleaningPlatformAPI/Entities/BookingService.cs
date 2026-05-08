using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entites
{
    [Table("BookingServices")]
    public class BookingService
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public int ServiceCatalogId { get; set; }

        public decimal? EstimatedPrice { get; set; }
        public decimal? FinalPrice { get; set; }

        public decimal Quantity { get; set; } = 1;
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }

        [ForeignKey(nameof(ServiceCatalogId))]
        public ServiceCatalog ServiceCatalog { get; set; }
    }
}