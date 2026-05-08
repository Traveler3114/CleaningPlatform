using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("Contacts")]
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }

        [Required, MaxLength(200)]
        public string ContactName { get; set; }

        [MaxLength(100)]
        public string? Role { get; set; }

        [Required, MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; }
    }
}
