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

    public ScheduleModel(ScheduleManager scheduleManager)
    {
        _scheduleManager = scheduleManager;
    }

    public List<WeeklyScheduleResponse> Schedule { get; set; } = [];

    [BindProperty]
    public WeeklyScheduleRequest DayInput { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Schedule = await _scheduleManager.GetScheduleAsync();
    }

    public async Task<IActionResult> OnPostAddDayAsync()
    {
        if (!User.HasPermission(PermissionKeys.ActionsScheduleEdit))
            return Forbid();

        var result = await _scheduleManager.CreateDayAsync(DayInput);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateDayAsync(int dayOfWeek)
    {
        if (!User.HasPermission(PermissionKeys.ActionsScheduleEdit))
            return Forbid();

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
        if (!User.HasPermission(PermissionKeys.ActionsScheduleEdit))
            return Forbid();

        var result = await _scheduleManager.DeleteDayAsync(dayOfWeek);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }
}
