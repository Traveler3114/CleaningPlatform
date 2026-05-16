using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesDaily)]
public class IndexModel : PageModel
{
    private readonly BookingManager _bookingManager;
    private readonly AvailabilityManager _availabilityManager;
    private readonly ReportingManager _reportingManager;

    public IndexModel(
        BookingManager bookingManager,
        AvailabilityManager availabilityManager,
        ReportingManager reportingManager)
    {
        _bookingManager = bookingManager;
        _availabilityManager = availabilityManager;
        _reportingManager = reportingManager;
    }

    public List<BookingResponse>        TodaysBookings  { get; set; } = [];
    public List<AvailabilityResponse>   Slots           { get; set; } = [];
    public int KpiPending              { get; set; }
    public int KpiConfirmed            { get; set; }
    public int KpiCompletedThisMonth   { get; set; }
    public DashboardSummaryResponse DashboardSummary { get; set; } =
        new(null, null, null, new OverdueInvoiceSummaryResponse(0, 0, 0, 0));

    [BindProperty(SupportsGet = true)]
    public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;

    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    private async Task LoadAsync(CancellationToken ct)
    {
        TodaysBookings   = await _bookingManager.GetBookingsAsync(SelectedDate, ct);
        Slots            = await _availabilityManager.GetSlotsAsync(SelectedDate, ct);
        DashboardSummary = await _reportingManager.GetDashboardSummaryAsync(ct);

        KpiPending = TodaysBookings.Count(b =>
            string.Equals(b.Status, nameof(BookingStatus.Pending), StringComparison.OrdinalIgnoreCase));
        KpiConfirmed = TodaysBookings.Count(b =>
            string.Equals(b.Status, nameof(BookingStatus.Confirmed), StringComparison.OrdinalIgnoreCase));
        KpiCompletedThisMonth = TodaysBookings.Count(b =>
            string.Equals(b.Status, nameof(BookingStatus.Completed), StringComparison.OrdinalIgnoreCase)
            && b.Date.Month == SelectedDate.Month && b.Date.Year == SelectedDate.Year);
    }
}