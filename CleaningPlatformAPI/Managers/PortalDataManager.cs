using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class PortalDataManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public PortalDataManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<PortalDashboardResponse> GetDashboardAsync(int clientId, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            throw new AppException("CLIENT_NOT_FOUND", "Client not found.", 404);

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
            .Include(b => b.Assignments).ThenInclude(a => a.Employee)
            .Include(b => b.Site)
            .Where(b => b.ClientId == clientId && b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.ScheduledDate).ThenBy(b => b.ScheduledTimeSlot)
            .Take(20)
            .ToListAsync(ct);

        var recentInvoices = await _db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .Take(5)
            .ToListAsync(ct);

        return new PortalDashboardResponse
        {
            UpcomingBookings = upcomingCount,
            CompletedBookings = completedCount,
            TotalSpent = totalSpent,
            OutstandingAmount = outstanding,
            UpcomingBookingList = upcomingBookings.Select(BookingMapper.ToDetailResponse).ToList(),
            RecentInvoices = recentInvoices.Select(InvoiceMapper.ToResponse).ToList()
        };
    }

    public async Task<List<BookingResponse>> GetBookingsAsync(int clientId, string? status, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            throw new AppException("CLIENT_NOT_FOUND", "Client not found.", 404);

        var query = _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee)
            .Include(b => b.Site)
            .Where(b => b.ClientId == clientId);

        if (status == "upcoming")
            query = query.Where(b => b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled);
        else if (status == "completed")
            query = query.Where(b => b.Status == BookingStatus.Completed);

        var bookings = await query
            .OrderByDescending(b => b.ScheduledDate).ThenByDescending(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);

        return bookings.Select(BookingMapper.ToDetailResponse).ToList();
    }

    public async Task<BookingResponse> GetBookingDetailAsync(int clientId, int bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee)
            .Include(b => b.Site)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.ClientId == clientId, ct);

        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", "Booking not found.", 404);

        return BookingMapper.ToDetailResponse(booking);
    }

    public async Task<List<InvoiceResponse>> GetInvoicesAsync(int clientId, CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            throw new AppException("CLIENT_NOT_FOUND", "Client not found.", 404);

        var invoices = await _db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(ct);

        return invoices.Select(InvoiceMapper.ToResponse).ToList();
    }

    public async Task<InvoiceResponse> GetInvoiceDetailAsync(int clientId, int invoiceId, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ClientId == clientId, ct);

        if (invoice is null)
            throw new AppException("INVOICE_NOT_FOUND", "Invoice not found.", 404);

        return InvoiceMapper.ToResponse(invoice);
    }

    public async Task<PortalProfileResponse> GetProfileAsync(int clientId, CancellationToken ct = default)
    {
        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);

        if (client is null)
            throw new AppException("CLIENT_NOT_FOUND", "Client not found.", 404);

        var primary = client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)
            ?? client.Contacts.FirstOrDefault(c => c.IsActive);

        var nameInitial = client.ClientName.Length > 0
            ? client.ClientName[0].ToString().ToUpper()
            : "?";

        return new PortalProfileResponse
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
        };
    }
}
