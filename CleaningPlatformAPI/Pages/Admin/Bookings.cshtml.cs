using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesBookings)]
public class BookingsModel : PageModel
{
    private readonly BookingManager _bookingManager;

    public BookingsModel(BookingManager bookingManager)
    {
        _bookingManager = bookingManager;
    }

    public List<BookingResponse> Bookings { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public DateTime? DateFilter { get; set; }

    public bool ShowAll { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success)
            ErrorMessage = result.Message;

        return DateFilter.HasValue
            ? RedirectToPage(new { dateFilter = DateFilter.Value.ToString("yyyy-MM-dd") })
            : RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
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
