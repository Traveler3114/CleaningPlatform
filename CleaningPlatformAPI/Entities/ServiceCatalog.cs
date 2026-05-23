using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("ServiceCatalog")]
    public class ServiceCatalog
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string CatalogCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public decimal? PriceAvg { get; set; }
        public decimal? DefaultMarginPct { get; set; }

        [Required, MaxLength(50)]
        public string ServiceType { get; set; } = "Vehicle";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
        public ICollection<SopTemplate> SopTemplates { get; set; } = new List<SopTemplate>();
    }
}
