using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesKanban)]
public class KanbanModel : PageModel
{
    private readonly KanbanManager _kanbanManager;
    private readonly BookingManager _bookingManager;

    public KanbanModel(KanbanManager kanbanManager, BookingManager bookingManager)
    {
        _kanbanManager = kanbanManager;
        _bookingManager = bookingManager;
    }

    public WeeklyBoardResponse Board { get; set; } = null!;
    public bool IsEmployeeView { get; set; }

    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public string? WeekStart { get; set; }
    [BindProperty(SupportsGet = true)] public string? DayDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? MonthDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? FilterEmployeeId { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public DateTime CurrentWeekStart { get; private set; }
    public DateTime CurrentDay { get; private set; }
    public DateTime CurrentMonth { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        IsEmployeeView = string.Equals(role, RoleNames.Employee, StringComparison.OrdinalIgnoreCase);

        if (IsEmployeeView)
        {
            var empId = User.GetEmployeeId();
            CurrentWeekStart = GetMondayOf(DateTime.UtcNow);
            Board = await _kanbanManager.GetEmployeeWeekAsync(empId!.Value, CurrentWeekStart, ct);
            View = "week";
            return;
        }

        CurrentWeekStart = GetMondayOf(
            DateTime.TryParse(WeekStart, out var ws) ? ws : DateTime.UtcNow);
        CurrentDay = DateTime.TryParse(DayDate, out var dd) ? dd.Date : DateTime.UtcNow.Date;
        var md = DateTime.TryParse(MonthDate, out var m) ? m : DateTime.UtcNow;
        CurrentMonth = new DateTime(md.Year, md.Month, 1);

        Board = View switch
        {
            "day"   => await _kanbanManager.GetWeekAsync(CurrentDay, FilterEmployeeId, ct),
            "month" => await _kanbanManager.GetWeekAsync(CurrentMonth, FilterEmployeeId, ct),
            _       => await _kanbanManager.GetWeekAsync(CurrentWeekStart, FilterEmployeeId, ct)
        };
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { view = View, weekStart = WeekStart, filterEmployeeId = FilterEmployeeId });
    }

    public async Task<IActionResult> OnPostAddAssignmentAsync(int id, int employeeId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.AddAssignmentAsync(id, employeeId, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { view = View, weekStart = WeekStart, filterEmployeeId = FilterEmployeeId });
    }

    private static DateTime GetMondayOf(DateTime date)
    {
        var d = date.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }
}
