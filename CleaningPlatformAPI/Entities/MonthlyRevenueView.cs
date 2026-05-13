using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class MonthlyRevenueView
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TotalSubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalRevenue { get; set; }
}
