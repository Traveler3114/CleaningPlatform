using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class InvoiceManager
{
    private static readonly string[] AllowedStatuses    = ["Draft", "Sent", "PartiallyPaid", "Paid", "Overdue", "WrittenOff"];
    private static readonly string[] AllowedPaymentMethods = ["BankTransfer", "Cash", "Card", "Other"];

    private readonly AppDbContext _db;

    public InvoiceManager(AppDbContext db) { _db = db; }

    public async Task<List<InvoiceSummaryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var invoices = await _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.Payments)
            .Include(i => i.InvoiceBookings)
            .OrderByDescending(i => i.IssueDate).ThenByDescending(i => i.Id)
            .ToListAsync(ct);
        return invoices.Select(InvoiceMapper.ToSummaryResponse).ToList();
    }

    public async Task<InvoiceDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Client)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Include(i => i.InvoiceBookings)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
        return invoice == null ? null : InvoiceMapper.ToDetailResponse(invoice);
    }

    public async Task<OperationResult<InvoiceDetailResponse>> CreateFromBookingAsync(int bookingId, int? createdByEmployeeId, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var booking = await _db.Bookings
            .Include(b => b.Client)
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking == null)
            return OperationResult<InvoiceDetailResponse>.Fail($"Booking #{bookingId} was not found.");

        if (booking.Status != BookingStatus.Completed)
            return OperationResult<InvoiceDetailResponse>.Fail($"Booking #{bookingId} cannot be invoiced — its current status is '{booking.Status}'. Only Completed bookings can generate an invoice.");

        var existingLink = await _db.InvoiceBookings.AsNoTracking()
            .FirstOrDefaultAsync(ib => ib.BookingId == bookingId, ct);
        if (existingLink != null)
            return OperationResult<InvoiceDetailResponse>.Fail($"Booking #{bookingId} is already linked to invoice #{existingLink.InvoiceId}. Open that invoice to record additional payments.");

        var now           = DateTime.UtcNow;
        var invoiceNumber = await GenerateInvoiceNumberAsync(ct);
        var issueDate     = now.Date;
        var dueDate       = issueDate.AddDays(ParsePaymentTermDays(booking.Client?.PaymentTerms));

        var invoice = new Invoice
        {
            InvoiceNumber        = invoiceNumber,
            ClientId             = booking.ClientId,
            IssueDate            = issueDate,
            DueDate              = dueDate,
            DiscountAmount       = 0,
            VatPct               = 0,
            Status               = "Draft",
            Notes                = $"Generated from booking #{booking.Id}",
            CreatedByEmployeeId  = createdByEmployeeId,
            CreatedAt            = now,
            UpdatedAt            = now
        };

        var lines = booking.BookingServices.Select(bs => new InvoiceLine
        {
            Description = bs.ServiceCatalog?.Name ?? $"Booking #{booking.Id} service",
            Quantity    = bs.Quantity <= 0 ? 1 : bs.Quantity,
            UnitPrice   = bs.FinalPrice ?? bs.EstimatedPrice ?? bs.ServiceCatalog?.PriceAvg ?? 0,
            DiscountPct = 0,
            VatPct      = 0,
            SourceType  = "Booking",
            SourceId    = booking.Id,
            CreatedAt   = now
        }).ToList();

        if (lines.Count == 0)
        {
            lines.Add(new InvoiceLine
            {
                Description = $"Booking #{booking.Id}",
                Quantity    = 1,
                UnitPrice   = 0,
                DiscountPct = 0,
                VatPct      = 0,
                SourceType  = "Booking",
                SourceId    = booking.Id,
                CreatedAt   = now
            });
        }

        invoice.Lines = lines;
        RecalculateTotals(invoice);

        _db.Invoices.Add(invoice);
        _db.InvoiceBookings.Add(new InvoiceBooking { Invoice = invoice, BookingId = booking.Id });

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var created = await GetByIdAsync(invoice.Id, ct);
        return created == null
            ? OperationResult<InvoiceDetailResponse>.Fail("Invoice was created but could not be loaded. Please refresh.")
            : OperationResult<InvoiceDetailResponse>.Ok(created);
    }

    public async Task<OperationResult<InvoiceDetailResponse>> RecordPaymentAsync(int invoiceId, RecordPaymentRequest request, int? recordedBy, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return OperationResult<InvoiceDetailResponse>.Fail("Payment amount must be greater than zero.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? "BankTransfer" : request.Method.Trim();
        if (!AllowedPaymentMethods.Contains(method))
            return OperationResult<InvoiceDetailResponse>.Fail($"'{method}' is not a valid payment method. Accepted values: {string.Join(", ", AllowedPaymentMethods)}.");

        var invoice = await _db.Invoices.Include(i => i.Payments).FirstOrDefaultAsync(i => i.Id == invoiceId, ct);
        if (invoice == null)
            return OperationResult<InvoiceDetailResponse>.Fail($"Invoice #{invoiceId} was not found.");

        var paymentDate = request.PaymentDate == default ? DateTime.UtcNow.Date : request.PaymentDate.Date;

        _db.Payments.Add(new Payment
        {
            InvoiceId   = invoiceId,
            PaymentDate = paymentDate,
            Amount      = request.Amount,
            Method      = method,
            Reference   = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim(),
            Notes       = string.IsNullOrWhiteSpace(request.Notes)     ? null : request.Notes.Trim(),
            RecordedBy  = recordedBy,
            CreatedAt   = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        var paidAmount = await _db.Payments.Where(p => p.InvoiceId == invoiceId).SumAsync(p => p.Amount, ct);
        invoice.Status    = paidAmount >= invoice.TotalAmount ? "Paid" : paidAmount > 0 ? "PartiallyPaid" : invoice.Status;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var updated = await GetByIdAsync(invoiceId, ct);
        return updated == null
            ? OperationResult<InvoiceDetailResponse>.Fail($"Invoice #{invoiceId} was not found.")
            : OperationResult<InvoiceDetailResponse>.Ok(updated);
    }

    public async Task<OperationResult<InvoiceDetailResponse>> UpdateStatusAsync(int invoiceId, string status, CancellationToken ct = default)
    {
        var trimmed = status?.Trim() ?? "";
        if (!AllowedStatuses.Contains(trimmed))
            return OperationResult<InvoiceDetailResponse>.Fail($"'{trimmed}' is not a valid invoice status. Accepted values: {string.Join(", ", AllowedStatuses)}.");

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, ct);
        if (invoice == null)
            return OperationResult<InvoiceDetailResponse>.Fail($"Invoice #{invoiceId} was not found.");

        invoice.Status    = trimmed;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var updated = await GetByIdAsync(invoiceId, ct);
        return updated == null
            ? OperationResult<InvoiceDetailResponse>.Fail($"Invoice #{invoiceId} was not found.")
            : OperationResult<InvoiceDetailResponse>.Ok(updated);
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct)
    {
        var year   = DateTime.UtcNow.Year;
        var result = await _db.Database.SqlQueryRaw<long>("SELECT NEXT VALUE FOR InvoiceNumberSeq").FirstAsync(ct);
        return $"INV-{year}-{result:0000}";
    }

    private static int ParsePaymentTermDays(string? paymentTerms)
    {
        if (string.IsNullOrWhiteSpace(paymentTerms)) return 15;
        var digits = new string(paymentTerms.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var days) && days > 0 ? days : 15;
    }

    private static void RecalculateTotals(Invoice invoice)
    {
        var subTotal  = 0m;
        var vatAmount = 0m;
        foreach (var line in invoice.Lines)
        {
            var qty         = line.Quantity  <= 0   ? 1   : line.Quantity;
            var price       = line.UnitPrice  < 0   ? 0   : line.UnitPrice;
            var discountPct = Math.Clamp(line.DiscountPct ?? 0, 0, 100);
            var lineVatPct  = Math.Clamp(line.VatPct, 0, 100);
            var net         = qty * price * (1 - discountPct / 100m);
            subTotal  += net;
            vatAmount += net * (lineVatPct / 100m);
        }
        invoice.SubTotal    = decimal.Round(subTotal, 2, MidpointRounding.AwayFromZero);
        invoice.VatAmount   = decimal.Round(vatAmount, 2, MidpointRounding.AwayFromZero);
        invoice.TotalAmount = decimal.Round(invoice.SubTotal - invoice.DiscountAmount + invoice.VatAmount, 2, MidpointRounding.AwayFromZero);
    }
}
