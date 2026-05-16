using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesBookings)]
public class BookingsModel : PageModel
{
    private readonly BookingManager _bookingManager;
    private readonly AppDbContext _db;

    public BookingsModel(BookingManager bookingManager, AppDbContext db)
    {
        _bookingManager = bookingManager;
        _db = db;
    }

    public List<BookingResponse> Bookings { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public DateTime? DateFilter { get; set; }

    public bool ShowAll { get; set; }
    public List<Client> Clients { get; set; } = [];
    public List<Site> Sites { get; set; } = [];
    public List<ServiceCatalog> Services { get; set; } = [];

    [BindProperty]
    public CreateAdminBookingRequest NewBooking { get; set; } = new() { Date = DateTime.UtcNow.Date, ServiceType = BookingServiceType.Vehicle, Hour = 9 };
    [BindProperty]
    public int? NewServiceCatalogId { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostCreateAdminAsync(CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsCreate))
            return Forbid();

        if (NewServiceCatalogId.HasValue)
            NewBooking.Services.Add(new AddServiceRequest { ServiceCatalogId = NewServiceCatalogId.Value, Quantity = 1 });

        var result = await _bookingManager.CreateAdminBookingAsync(NewBooking, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit))
            return Forbid();

        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success) ErrorMessage = result.Message;

        return DateFilter.HasValue
            ? RedirectToPage(new { dateFilter = DateFilter.Value.ToString("yyyy-MM-dd") })
            : RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Clients = await _db.Clients.Where(c => c.IsActive).OrderBy(c => c.ClientName).ToListAsync(ct);
        Sites = await _db.Sites.Include(s => s.Client).Where(s => s.IsActive).OrderBy(s => s.SiteName).ToListAsync(ct);
        Services = await _db.ServiceCatalog.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(ct);

        if (DateFilter.HasValue)
        {
            ShowAll = false;
            Bookings = await _bookingManager.GetBookingsAsync(DateFilter.Value, ct);
            return;
        }

        ShowAll = true;
        Bookings = await _bookingManager.GetAllBookingsAsync(ct);
    }
}
