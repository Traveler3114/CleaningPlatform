using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class InvoiceMapper
{
    public static InvoiceSummaryResponse ToSummaryResponse(Invoice invoice)
    {
        var paidAmount = invoice.Payments.Sum(p => p.Amount);
        var balanceDue = invoice.TotalAmount - paidAmount;

        return new InvoiceSummaryResponse
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            ClientId = invoice.ClientId,
            ClientName = invoice.Client?.ClientName ?? string.Empty,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = balanceDue < 0 ? 0 : balanceDue,
            Status = invoice.Status,
            BookingCount = invoice.InvoiceBookings.Count
        };
    }

    public static InvoiceDetailResponse ToDetailResponse(Invoice invoice)
    {
        var summary = ToSummaryResponse(invoice);

        return new InvoiceDetailResponse
        {
            Id = summary.Id,
            InvoiceNumber = summary.InvoiceNumber,
            ClientId = summary.ClientId,
            ClientName = summary.ClientName,
            IssueDate = summary.IssueDate,
            DueDate = summary.DueDate,
            TotalAmount = summary.TotalAmount,
            PaidAmount = summary.PaidAmount,
            BalanceDue = summary.BalanceDue,
            Status = summary.Status,
            BookingCount = summary.BookingCount,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            VatPct = invoice.VatPct,
            VatAmount = invoice.VatAmount,
            Notes = invoice.Notes,
            Lines = invoice.Lines
                .OrderBy(l => l.Id)
                .Select(l =>
                {
                    var discountPct = Math.Clamp(l.DiscountPct ?? 0, 0, 100);
                    var net = l.Quantity * l.UnitPrice * (1 - (discountPct / 100m));
                    var lineVat = net * (l.VatPct / 100m);

                    return new InvoiceLineResponse
                    {
                        Id = l.Id,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPct = l.DiscountPct,
                        VatPct = l.VatPct,
                        LineNetAmount = decimal.Round(net, 2, MidpointRounding.AwayFromZero),
                        LineVatAmount = decimal.Round(lineVat, 2, MidpointRounding.AwayFromZero),
                        LineTotalAmount = decimal.Round(net + lineVat, 2, MidpointRounding.AwayFromZero),
                        SourceType = l.SourceType,
                        SourceId = l.SourceId
                    };
                })
                .ToList(),
            Payments = invoice.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    Method = p.Method,
                    Reference = p.Reference,
                    Notes = p.Notes
                })
                .ToList()
        };
    }
}
