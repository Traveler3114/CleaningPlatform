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
    public KanbanModel(KanbanManager kanbanManager, BookingManager bookingManager) { _kanbanManager = kanbanManager; _bookingManager = bookingManager; }

    public WeeklyBoardResponse Board { get; set; } = new(DateTime.UtcNow.Date, [], []);
    public bool IsEmployeeView { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? WeekStart { get; set; }
    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public int? EmployeeId { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var start = GetWeekStart(WeekStart ?? DateTime.UtcNow.Date);
        WeekStart = start;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        IsEmployeeView = string.Equals(role, RoleNames.Employee, StringComparison.OrdinalIgnoreCase);
        if (IsEmployeeView) Board = await _kanbanManager.GetEmployeeWeekAsync(User.GetEmployeeId(), start, ct);
        else Board = await _kanbanManager.GetWeekAsync(start, EmployeeId, ct);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { weekStart = WeekStart?.ToString("yyyy-MM-dd"), view = View, employeeId = EmployeeId });
    }

    private static DateTime GetWeekStart(DateTime d)
    {
        var day = ((int)d.DayOfWeek + 6) % 7;
        return d.Date.AddDays(-day);
    }
}
