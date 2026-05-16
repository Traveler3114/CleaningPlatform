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
    private readonly DateOverrideManager _dateOverrideManager;
    private readonly ReportingManager _reportingManager;

    public IndexModel(
        BookingManager bookingManager,
        AvailabilityManager availabilityManager,
        DateOverrideManager dateOverrideManager,
        ReportingManager reportingManager)
    {
        _bookingManager      = bookingManager;
        _availabilityManager = availabilityManager;
        _dateOverrideManager = dateOverrideManager;
        _reportingManager    = reportingManager;
    }

    public List<BookingResponse>        TodaysBookings  { get; set; } = [];
    public List<AvailabilityResponse>   Slots           { get; set; } = [];
    public DateOverrideResponse?        TodayOverride   { get; set; }
    public int KpiPending              { get; set; }
    public int KpiConfirmed            { get; set; }
    public int KpiCompletedThisMonth   { get; set; }
    public DashboardSummaryResponse DashboardSummary { get; set; } =
        new(null, null, null, new OverdueInvoiceSummaryResponse(0, 0, 0, 0));

    [BindProperty(SupportsGet = true)]
    public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;

    [BindProperty] public int?  OverrideStartHour { get; set; }
    [BindProperty] public int?  OverrideEndHour   { get; set; }
    [BindProperty] public int?  OverrideCapacity  { get; set; }
    [BindProperty] public bool  OverrideClosed    { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveOverrideAsync(CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit))
            return Forbid();

        var result = await _dateOverrideManager.CreateOverrideAsync(new DateOverrideRequest
        {
            Date          = SelectedDate.Date,
            StartHour     = OverrideStartHour,
            EndHour       = OverrideEndHour,
            Capacity      = OverrideCapacity,
            IsFullyClosed = OverrideClosed
        }, ct);

        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { selectedDate = SelectedDate.ToString("yyyy-MM-dd") });
    }

    public async Task<IActionResult> OnPostDeleteOverrideAsync(int id, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit))
            return Forbid();

        var result = await _dateOverrideManager.DeleteOverrideAsync(id, ct);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { selectedDate = SelectedDate.ToString("yyyy-MM-dd") });
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        TodaysBookings   = await _bookingManager.GetBookingsAsync(SelectedDate, ct);
        Slots            = await _availabilityManager.GetSlotsAsync(SelectedDate, ct);
        DashboardSummary = await _reportingManager.GetDashboardSummaryAsync(ct);

        var overrides = await _dateOverrideManager.GetOverridesAsync(ct);
        TodayOverride = overrides.FirstOrDefault(o => o.Date.Date == SelectedDate.Date);

        if (TodayOverride != null)
        {
            OverrideStartHour = TodayOverride.StartHour;
            OverrideEndHour   = TodayOverride.EndHour;
            OverrideCapacity  = TodayOverride.Capacity;
            OverrideClosed    = TodayOverride.IsFullyClosed;
        }

        KpiPending = TodaysBookings.Count(b =>
            string.Equals(b.Status, nameof(BookingStatus.Pending), StringComparison.OrdinalIgnoreCase));
        KpiConfirmed = TodaysBookings.Count(b =>
            string.Equals(b.Status, nameof(BookingStatus.Confirmed), StringComparison.OrdinalIgnoreCase));
        KpiCompletedThisMonth = TodaysBookings.Count(b =>
            string.Equals(b.Status, nameof(BookingStatus.Completed), StringComparison.OrdinalIgnoreCase)
            && b.Date.Month == SelectedDate.Month && b.Date.Year == SelectedDate.Year);
    }
}
