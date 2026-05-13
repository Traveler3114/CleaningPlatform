using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class OverdueInvoiceSummaryView
{
    public decimal? TotalOverdueAmount { get; set; }
    public int OverdueInvoiceCount { get; set; }
    public decimal? AvgOverdueAmount { get; set; }
    public int? MaxDaysOverdue { get; set; }
}
