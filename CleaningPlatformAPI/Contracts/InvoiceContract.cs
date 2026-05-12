namespace CleaningPlatformAPI.Contracts;

public class InvoiceSummaryResponse
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

public class InvoiceDetailResponse : InvoiceSummaryResponse
{
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatPct { get; set; }
    public decimal VatAmount { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceLineResponse> Lines { get; set; } = new();
    public List<PaymentResponse> Payments { get; set; } = new();
}

public class InvoiceLineResponse
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPct { get; set; }
    public decimal VatPct { get; set; }
    public decimal LineNetAmount { get; set; }
    public decimal LineVatAmount { get; set; }
    public decimal LineTotalAmount { get; set; }
    public string? SourceType { get; set; }
    public int? SourceId { get; set; }
}

public class PaymentResponse
{
    public int Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public record CreateInvoiceFromBookingRequest
{
    public int BookingId { get; set; }
}

public record RecordPaymentRequest
{
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow.Date;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "BankTransfer";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public record UpdateInvoiceStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
