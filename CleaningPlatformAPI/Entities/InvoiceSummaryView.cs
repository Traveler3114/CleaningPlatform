using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class InvoiceSummaryView
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public decimal AmountOutstanding { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    public string? CreatedBy { get; set; }
}
