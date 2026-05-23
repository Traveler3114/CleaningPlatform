using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;
[Table("InvoiceLines")]
public class InvoiceLine
{
    [Key]
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPct { get; set; }
    public decimal VatPct { get; set; }

    [MaxLength(50)]
    public string? SourceType { get; set; }

    public int? SourceId { get; set; }
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public Invoice Invoice { get; set; } = null!;
}
