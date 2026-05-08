using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string ClientName { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }   // OneTime, RepeatIndividual, RepeatBusiness

        [MaxLength(50)]
        public string? Oib { get; set; }

        [MaxLength(100)]
        public string? PaymentTerms { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
