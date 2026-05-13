using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class TopClientView
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalPaid { get; set; }
}
