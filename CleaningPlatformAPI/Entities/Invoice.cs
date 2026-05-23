using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;
[Table("Invoices")]
public class Invoice
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public int ClientId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatPct { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Draft";

    public string? Notes { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(ClientId))]
    public Client Client { get; set; } = null!;

    [ForeignKey(nameof(CreatedByEmployeeId))]
    public Employee? CreatedByEmployee { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InvoiceBooking> InvoiceBookings { get; set; } = new List<InvoiceBooking>();
}
