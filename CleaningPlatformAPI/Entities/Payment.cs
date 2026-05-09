using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string Method { get; set; } = "BankTransfer";

        [MaxLength(200)]
        public string? Reference { get; set; }

        public string? Notes { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        [ForeignKey(nameof(RecordedBy))]
        public Employee? RecordedByEmployee { get; set; }
    }
}
