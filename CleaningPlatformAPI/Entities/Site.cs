using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("Sites")]
    public class Site
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }

        [Required, MaxLength(200)]
        public string SiteName { get; set; }

        [Required, MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(50)]
        public string? SiteType { get; set; }

        public decimal? FloorAreaM2 { get; set; }
        public string? AccessNotes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
