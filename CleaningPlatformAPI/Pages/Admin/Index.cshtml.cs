using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesDaily)]
public class IndexModel : PageModel
{
    private readonly BookingManager _bookingManager;
    private readonly AvailabilityManager _availabilityManager;
    private readonly DateOverrideManager _dateOverrideManager;

    public IndexModel(BookingManager bookingManager, AvailabilityManager availabilityManager, DateOverrideManager dateOverrideManager)
    {
        _bookingManager = bookingManager;
        _availabilityManager = availabilityManager;
        _dateOverrideManager = dateOverrideManager;
    }

    public List<BookingDto> TodaysBookings { get; set; } = [];
    public List<AvailabilityDto> Slots { get; set; } = [];
    public DateOverrideDto? TodayOverride { get; set; }
    public int KpiPending { get; set; }
    public int KpiConfirmed { get; set; }
    public int KpiCompletedThisMonth { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    [BindProperty]
    public int? OverrideStartHour { get; set; }

    [BindProperty]
    public int? OverrideEndHour { get; set; }

    [BindProperty]
    public int? OverrideCapacity { get; set; }

    [BindProperty]
    public bool OverrideClosed { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSaveOverrideAsync()
    {
        if (!User.HasPermission(PermissionKeys.ActionsOverrideManage))
            return Forbid();

        var result = await _dateOverrideManager.CreateOverrideAsync(new DateOverrideDto
        {
            Date = SelectedDate.Date,
            StartHour = OverrideStartHour,
            EndHour = OverrideEndHour,
            Capacity = OverrideCapacity,
            IsFullyClosed = OverrideClosed
        });

        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { selectedDate = SelectedDate.ToString("yyyy-MM-dd") });
    }

    public async Task<IActionResult> OnPostDeleteOverrideAsync(int id)
    {
        if (!User.HasPermission(PermissionKeys.ActionsOverrideManage))
            return Forbid();

        var result = await _dateOverrideManager.DeleteOverrideAsync(id);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { selectedDate = SelectedDate.ToString("yyyy-MM-dd") });
    }

    private async Task LoadAsync()
    {
        TodaysBookings = await _bookingManager.GetBookingsAsync(SelectedDate);
        Slots = await _availabilityManager.GetSlotsAsync(SelectedDate);

        var overrides = await _dateOverrideManager.GetOverridesAsync();
        TodayOverride = overrides.FirstOrDefault(o => o.Date.Date == SelectedDate.Date);

        if (TodayOverride != null)
        {
            OverrideStartHour = TodayOverride.StartHour;
            OverrideEndHour = TodayOverride.EndHour;
            OverrideCapacity = TodayOverride.Capacity;
            OverrideClosed = TodayOverride.IsFullyClosed;
        }

        KpiPending = TodaysBookings.Count(b => string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));
        KpiConfirmed = TodaysBookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase));
        KpiCompletedThisMonth = TodaysBookings.Count(b =>
            string.Equals(b.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
            b.Date.Month == SelectedDate.Month && b.Date.Year == SelectedDate.Year);
    }
}
