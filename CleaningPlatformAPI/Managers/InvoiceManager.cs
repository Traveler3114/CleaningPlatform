using System.Globalization;
using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class InvoiceManager
{
    private static readonly string[] AllowedStatuses = ["Draft", "Sent", "PartiallyPaid", "Paid", "Overdue", "WrittenOff"];
    private static readonly string[] AllowedPaymentMethods = ["BankTransfer", "Cash", "Card", "Other"];

    private readonly AppDbContext _db;

    public InvoiceManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<InvoiceSummaryDto>> GetAllAsync()
    {
        var invoices = await _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.Payments)
            .Include(i => i.InvoiceBookings)
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();

        return invoices.Select(MapToSummaryDto).ToList();
    }

    public async Task<InvoiceDetailDto?> GetByIdAsync(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Include(i => i.InvoiceBookings)
            .FirstOrDefaultAsync(i => i.Id == id);

        return invoice == null ? null : MapToDetailDto(invoice);
    }

    public async Task<OperationResult<InvoiceDetailDto>> CreateFromBookingAsync(int bookingId, int? createdByEmployeeId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Client)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return OperationResult<InvoiceDetailDto>.Fail("Booking not found.");

        if (!string.Equals(booking.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            return OperationResult<InvoiceDetailDto>.Fail("Only completed bookings can be invoiced.");

        var existingLink = await _db.InvoiceBookings
            .AsNoTracking()
            .FirstOrDefaultAsync(ib => ib.BookingId == bookingId);

        if (existingLink != null)
            return OperationResult<InvoiceDetailDto>.Fail($"Booking already invoiced (Invoice #{existingLink.InvoiceId}).");

        var now = DateTime.UtcNow;
        var invoiceNumber = await GenerateInvoiceNumberAsync();
        var issueDate = now.Date;
        var dueDate = issueDate.AddDays(ParsePaymentTermDays(booking.Client?.PaymentTerms));

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            ClientId = booking.ClientId,
            IssueDate = issueDate,
            DueDate = dueDate,
            DiscountAmount = 0,
            VatPct = 0,
            Status = "Draft",
            Notes = $"Generated from booking #{booking.Id}",
            CreatedByEmployeeId = createdByEmployeeId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var lines = booking.BookingServices.Select(bs => new InvoiceLine
        {
            Description = bs.ServiceCatalog?.Name ?? $"Booking #{booking.Id} service",
            Quantity = bs.Quantity <= 0 ? 1 : bs.Quantity,
            UnitPrice = bs.FinalPrice ?? bs.EstimatedPrice ?? bs.ServiceCatalog?.PriceAvg ?? 0,
            DiscountPct = 0,
            VatPct = 0,
            SourceType = "Booking",
            SourceId = booking.Id,
            CreatedAt = now
        }).ToList();

        if (lines.Count == 0)
        {
            lines.Add(new InvoiceLine
            {
                Description = $"Booking #{booking.Id}",
                Quantity = 1,
                UnitPrice = 0,
                DiscountPct = 0,
                VatPct = 0,
                SourceType = "Booking",
                SourceId = booking.Id,
                CreatedAt = now
            });
        }

        invoice.Lines = lines;
        RecalculateTotals(invoice);

        _db.Invoices.Add(invoice);
        _db.InvoiceBookings.Add(new InvoiceBooking
        {
            Invoice = invoice,
            BookingId = booking.Id
        });

        await _db.SaveChangesAsync();

        var created = await GetByIdAsync(invoice.Id);
        return created == null
            ? OperationResult<InvoiceDetailDto>.Fail("Failed to load created invoice.")
            : OperationResult<InvoiceDetailDto>.Ok(created);
    }

    public async Task<OperationResult<InvoiceDetailDto>> RecordPaymentAsync(int invoiceId, RecordPaymentDto dto, int? recordedBy)
    {
        if (dto.Amount <= 0)
            return OperationResult<InvoiceDetailDto>.Fail("Payment amount must be greater than zero.");

        var method = string.IsNullOrWhiteSpace(dto.Method) ? "BankTransfer" : dto.Method.Trim();
        if (!AllowedPaymentMethods.Contains(method))
            return OperationResult<InvoiceDetailDto>.Fail("Invalid payment method.");

        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return OperationResult<InvoiceDetailDto>.Fail("Invoice not found.");

        var paymentDate = dto.PaymentDate == default ? DateTime.UtcNow.Date : dto.PaymentDate.Date;

        _db.Payments.Add(new Payment
        {
            InvoiceId = invoiceId,
            PaymentDate = paymentDate,
            Amount = dto.Amount,
            Method = method,
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            RecordedBy = recordedBy,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var paidAmount = await _db.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .SumAsync(p => p.Amount);

        if (paidAmount >= invoice.TotalAmount)
        {
            invoice.Status = "Paid";
        }
        else if (paidAmount > 0)
        {
            invoice.Status = "PartiallyPaid";
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var updated = await GetByIdAsync(invoiceId);
        return updated == null
            ? OperationResult<InvoiceDetailDto>.Fail("Invoice not found.")
            : OperationResult<InvoiceDetailDto>.Ok(updated);
    }

    public async Task<OperationResult<InvoiceDetailDto>> UpdateStatusAsync(int invoiceId, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !AllowedStatuses.Contains(status.Trim()))
            return OperationResult<InvoiceDetailDto>.Fail("Invalid invoice status.");

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (invoice == null)
            return OperationResult<InvoiceDetailDto>.Fail("Invoice not found.");

        invoice.Status = status.Trim();
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var updated = await GetByIdAsync(invoiceId);
        return updated == null
            ? OperationResult<InvoiceDetailDto>.Fail("Invoice not found.")
            : OperationResult<InvoiceDetailDto>.Ok(updated);
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var maxInvoiceNumber = await _db.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .Select(i => i.InvoiceNumber)
            .ToListAsync();

        var maxSequence = maxInvoiceNumber
            .Select(number => number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? number[prefix.Length..]
                : string.Empty)
            .Select(raw => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{(maxSequence + 1):0000}";
    }

    private static int ParsePaymentTermDays(string? paymentTerms)
    {
        if (string.IsNullOrWhiteSpace(paymentTerms))
            return 15;

        var digits = new string(paymentTerms.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out var days) && days > 0)
            return days;

        return 15;
    }

    private static void RecalculateTotals(Invoice invoice)
    {
        var subTotal = 0m;
        var vatAmount = 0m;

        foreach (var line in invoice.Lines)
        {
            var quantity = line.Quantity <= 0 ? 1 : line.Quantity;
            var unitPrice = line.UnitPrice < 0 ? 0 : line.UnitPrice;
            var discountPct = Math.Clamp(line.DiscountPct ?? 0, 0, 100);
            var lineVatPct = Math.Clamp(line.VatPct, 0, 100);

            var net = quantity * unitPrice * (1 - (discountPct / 100m));
            var lineVat = net * (lineVatPct / 100m);

            subTotal += net;
            vatAmount += lineVat;
        }

        invoice.SubTotal = decimal.Round(subTotal, 2, MidpointRounding.AwayFromZero);
        invoice.VatAmount = decimal.Round(vatAmount, 2, MidpointRounding.AwayFromZero);
        invoice.TotalAmount = decimal.Round(invoice.SubTotal - invoice.DiscountAmount + invoice.VatAmount, 2, MidpointRounding.AwayFromZero);
    }

    private static InvoiceSummaryDto MapToSummaryDto(Invoice invoice)
    {
        var paidAmount = invoice.Payments.Sum(p => p.Amount);
        var balanceDue = invoice.TotalAmount - paidAmount;

        return new InvoiceSummaryDto
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

    private static InvoiceDetailDto MapToDetailDto(Invoice invoice)
    {
        var summary = MapToSummaryDto(invoice);

        return new InvoiceDetailDto
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

                    return new InvoiceLineDto
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
                .Select(p => new PaymentDto
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
