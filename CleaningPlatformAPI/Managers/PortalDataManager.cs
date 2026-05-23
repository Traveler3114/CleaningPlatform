using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;

namespace CleaningPlatformAPI.Managers;

public class PortalDataManager
{
    private readonly AppDbContext _db;

    public PortalDataManager(AppDbContext db) { _db = db; }

    public async Task<OperationResult<PortalDashboardResponse>> GetDashboardAsync(int clientId, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return OperationResult<PortalDashboardResponse>.Fail("Client not found.");

        var upcomingCount = await _db.Bookings
            .CountAsync(b => b.ClientId == clientId && b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled, ct);

        var completedCount = await _db.Bookings
            .CountAsync(b => b.ClientId == clientId && b.Status == BookingStatus.Completed, ct);

        var totalSpent = await _db.Invoices
            .Where(i => i.ClientId == clientId && i.Status == "Paid")
            .SumAsync(i => (decimal?)i.TotalAmount, ct) ?? 0;

        var outstanding = await _db.InvoiceSummaryViews
            .Where(v => v.ClientId == clientId && (v.Status == "Sent" || v.Status == "PartiallyPaid" || v.Status == "Overdue"))
            .SumAsync(v => (decimal?)v.AmountOutstanding, ct) ?? 0;

        var upcomingBookings = await _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .Include(b => b.Site)
            .Where(b => b.ClientId == clientId && b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.ScheduledDate).ThenBy(b => b.ScheduledTimeSlot)
            .Take(20)
            .ToListAsync(ct);

        var recentInvoices = await _db.Invoices
            .Include(i => i.Payments)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .Take(5)
            .ToListAsync(ct);

        return OperationResult<PortalDashboardResponse>.Ok(new PortalDashboardResponse
        {
            UpcomingBookings = upcomingCount,
            CompletedBookings = completedCount,
            TotalSpent = totalSpent,
            OutstandingAmount = outstanding,
            UpcomingBookingList = upcomingBookings.Select(MapBookingSummary).ToList(),
            RecentInvoices = recentInvoices.Select(MapInvoiceSummary).ToList()
        });
    }

    public async Task<OperationResult<List<PortalBookingSummary>>> GetBookingsAsync(int clientId, string? status, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return OperationResult<List<PortalBookingSummary>>.Fail("Client not found.");

        var query = _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .Include(b => b.Site)
            .Where(b => b.ClientId == clientId);

        if (status == "upcoming")
            query = query.Where(b => b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled);
        else if (status == "completed")
            query = query.Where(b => b.Status == BookingStatus.Completed);

        var bookings = await query
            .OrderByDescending(b => b.ScheduledDate).ThenByDescending(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);

        return OperationResult<List<PortalBookingSummary>>.Ok(bookings.Select(MapBookingSummary).ToList());
    }

    public async Task<OperationResult<PortalBookingDetailResponse>> GetBookingDetailAsync(int clientId, int bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee)
            .Include(b => b.Site)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.ClientId == clientId, ct);

        if (booking is null)
            return OperationResult<PortalBookingDetailResponse>.Fail("Booking not found.");

        return OperationResult<PortalBookingDetailResponse>.Ok(new PortalBookingDetailResponse
        {
            Id = booking.Id,
            Date = booking.ScheduledDate,
            Hour = booking.ScheduledTimeSlot?.Hours ?? 0,
            ServiceType = booking.ServiceType.ToString(),
            Status = booking.Status.ToString(),
            SiteName = booking.Site?.SiteName,
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt,
            Services = booking.BookingServices.Select(bs => new PortalBookingService
            {
                Name = bs.ServiceCatalog?.Name ?? string.Empty,
                Quantity = bs.Quantity,
                EstimatedPrice = bs.EstimatedPrice,
                FinalPrice = bs.FinalPrice
            }).ToList(),
            AssignedEmployees = booking.Assignments.Select(a => new PortalAssignedEmployee
            {
                FullName = $"{a.Employee.FirstName} {a.Employee.LastName}".Trim()
            }).ToList()
        });
    }

    public async Task<OperationResult<List<PortalInvoiceSummary>>> GetInvoicesAsync(int clientId, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return OperationResult<List<PortalInvoiceSummary>>.Fail("Client not found.");

        var invoices = await _db.Invoices
            .Include(i => i.Payments)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(ct);

        return OperationResult<List<PortalInvoiceSummary>>.Ok(invoices.Select(MapInvoiceSummary).ToList());
    }

    public async Task<OperationResult<PortalInvoiceDetailResponse>> GetInvoiceDetailAsync(int clientId, int invoiceId, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ClientId == clientId, ct);

        if (invoice is null)
            return OperationResult<PortalInvoiceDetailResponse>.Fail("Invoice not found.");

        var paidAmount = invoice.Payments.Sum(p => p.Amount);
        var balanceDue = Math.Max(invoice.TotalAmount - paidAmount, 0);

        return OperationResult<PortalInvoiceDetailResponse>.Ok(new PortalInvoiceDetailResponse
        {
            Id = invoice.Id,
            Number = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            VatPct = invoice.VatPct,
            VatAmount = invoice.VatAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = balanceDue,
            Lines = invoice.Lines.OrderBy(l => l.Id).Select(l => new PortalInvoiceLine
            {
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList(),
            Payments = invoice.Payments.OrderByDescending(p => p.PaymentDate).Select(p => new PortalPayment
            {
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                Method = p.Method,
                Reference = p.Reference
            }).ToList()
        });
    }

    public async Task<OperationResult<PortalProfileResponse>> GetProfileAsync(int clientId, CancellationToken ct = default)
    {
        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);

        if (client is null)
            return OperationResult<PortalProfileResponse>.Fail("Client not found.");

        var primary = client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)
            ?? client.Contacts.FirstOrDefault(c => c.IsActive);

        var nameInitial = client.ClientName.Length > 0
            ? client.ClientName[0].ToString().ToUpper()
            : "?";

        return OperationResult<PortalProfileResponse>.Ok(new PortalProfileResponse
        {
            Id = client.Id,
            Name = client.ClientName,
            Email = primary?.Email,
            Phone = primary?.Phone,
            Company = client.Type == "Business" ? client.ClientName : null,
            Since = client.CreatedAt,
            AvatarInitial = nameInitial,
            Sites = client.Sites
                .OrderByDescending(s => s.IsActive)
                .ThenBy(s => s.SiteName)
                .Select(s => new PortalSite
                {
                    Id = s.Id,
                    Name = s.SiteName,
                    Address = s.Address,
                    City = s.City,
                    SiteType = s.SiteType,
                    AccessNotes = s.AccessNotes
                }).ToList()
        });
    }

    private static PortalBookingSummary MapBookingSummary(Booking b)
    {
        var serviceNames = string.Join(", ", b.BookingServices.Select(bs => bs.ServiceCatalog?.Name).Where(n => n is not null));
        var estimatedTotal = b.BookingServices.Sum(bs => bs.EstimatedPrice ?? bs.FinalPrice ?? 0);

        return new PortalBookingSummary
        {
            Id = b.Id,
            Date = b.ScheduledDate,
            Hour = b.ScheduledTimeSlot?.Hours ?? 0,
            ServiceType = b.ServiceType.ToString(),
            Status = b.Status.ToString(),
            SiteName = b.Site?.SiteName,
            Services = serviceNames,
            EstimatedTotal = estimatedTotal
        };
    }

    private static PortalInvoiceSummary MapInvoiceSummary(Invoice invoice)
    {
        return new PortalInvoiceSummary
        {
            Id = invoice.Id,
            Number = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            TotalAmount = invoice.TotalAmount
        };
    }
}
