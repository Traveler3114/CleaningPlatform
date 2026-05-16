using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesSchedule)]
public class ScheduleModel : PageModel
{
    private readonly ScheduleManager _scheduleManager;
    private readonly DateOverrideManager _dateOverrideManager;

    public ScheduleModel(ScheduleManager scheduleManager, DateOverrideManager dateOverrideManager)
    {
        _scheduleManager = scheduleManager;
        _dateOverrideManager = dateOverrideManager;
    }

    public List<WeeklyScheduleResponse> Schedule { get; set; } = [];
    public List<DateOverrideResponse> DateOverrides { get; set; } = [];

    [BindProperty] public WeeklyScheduleRequest DayInput { get; set; } = new();

    // Date override bindings
    [BindProperty] public int? OverrideId { get; set; }
    [BindProperty] public DateTime OverrideDate { get; set; }
    [BindProperty] public int? OverrideStartHour { get; set; }
    [BindProperty] public int? OverrideEndHour { get; set; }
    [BindProperty] public int? OverrideCapacity { get; set; }
    [BindProperty] public bool OverrideClosed { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Schedule = await _scheduleManager.GetScheduleAsync();
        DateOverrides = await _dateOverrideManager.GetOverridesAsync(ct);
    }

    public async Task<IActionResult> OnPostAddDayAsync()
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit)) return Forbid();
        var result = await _scheduleManager.CreateDayAsync(DayInput);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateDayAsync(int dayOfWeek)
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit)) return Forbid();
        var result = await _scheduleManager.UpdateDayAsync(dayOfWeek, new UpdateWeeklyScheduleRequest
        {
            StartHour = DayInput.StartHour,
            EndHour = DayInput.EndHour,
            Capacity = DayInput.Capacity
        });
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteDayAsync(int dayOfWeek)
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit)) return Forbid();
        var result = await _scheduleManager.DeleteDayAsync(dayOfWeek);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveOverrideAsync(CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit)) return Forbid();

        var request = new DateOverrideRequest
        {
            Date = OverrideDate,
            StartHour = OverrideStartHour,
            EndHour = OverrideEndHour,
            Capacity = OverrideCapacity,
            IsFullyClosed = OverrideClosed
        };

        OperationResult<DateOverrideResponse> result;
        if (OverrideId.HasValue && OverrideId.Value > 0)
            result = await _dateOverrideManager.UpdateOverrideAsync(OverrideId.Value, request, ct);
        else
            result = await _dateOverrideManager.CreateOverrideAsync(request, ct);

        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteOverrideAsync(int id, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ScheduleEdit)) return Forbid();
        var result = await _dateOverrideManager.DeleteOverrideAsync(id, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }
}