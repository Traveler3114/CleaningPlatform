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

    // ── Computed navigation helpers used by the view ──────────────────────

    /// <summary>The resolved week start date (always a Monday).</summary>
    public DateTime WeekStartDate { get; private set; }

    /// <summary>The resolved day for day-view navigation.</summary>
    public DateTime? DayView { get; private set; }

    /// <summary>First hour shown on the calendar grid.</summary>
    public int GridStartHour { get; private set; } = 7;

    /// <summary>Last hour (exclusive) shown on the calendar grid.</summary>
    public int GridEndHour { get; private set; } = 19;

    // Navigation dates
    public DateTime PrevWeek  => WeekStartDate.AddDays(-7);
    public DateTime NextWeek  => WeekStartDate.AddDays(7);
    public DateTime PrevDay   => (DayView ?? WeekStartDate).AddDays(-1);
    public DateTime NextDay   => (DayView ?? WeekStartDate).AddDays(1);
    public DateTime PrevMonth => new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1).AddMonths(-1);
    public DateTime NextMonth => new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1).AddMonths(1);

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (IsEmployeeView)
        {
            var empId = User.GetEmployeeId();
            // Always use the current week – ignore any WeekStart from the query string
            WeekStartDate = GetMondayOf(DateTime.UtcNow);
            Board = await _kanbanManager.GetEmployeeWeekAsync(empId!.Value, WeekStartDate, ct);
            View = "week";
            ComputeGridHours();
            return;
        }
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

    // ── Helpers ───────────────────────────────────────────────────────────

    private void ComputeGridHours()
    {
        // Derive grid bounds from the board days so the calendar always shows
        // relevant hours without unnecessary empty rows.
        if (Board?.Days == null || !Board.Days.Any())
            return;

        var allBookings = Board.Days.SelectMany(d => d.Bookings).ToList();
        if (!allBookings.Any())
            return;

        var minHour = allBookings.Min(b => b.Hour);
        var maxHour = allBookings.Max(b => b.Hour);

        // Pad by one hour on each side and clamp to reasonable limits.
        GridStartHour = Math.Max(0,  minHour - 1);
        GridEndHour   = Math.Min(24, maxHour + 2);   // +2 so the last booking row is visible
    }

    private static DateTime GetMondayOf(DateTime date)
    {
        var d = date.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }
}
