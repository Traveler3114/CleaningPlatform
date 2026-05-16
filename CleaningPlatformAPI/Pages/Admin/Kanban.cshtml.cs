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

    // ── Shared properties ──────────────────────────────────────────────────
    public bool IsEmployeeView { get; set; }

    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public string? WeekStart { get; set; }
    [BindProperty(SupportsGet = true)] public string? DayDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? FilterEmployeeId { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    // ── Employee view property (existing, unchanged) ───────────────────────
    public WeeklyBoardResponse Board { get; private set; } = null!;

    // ── Admin resource grid property (new) ────────────────────────────────
    public ResourceGridResponse? ResourceGrid { get; private set; }

    // ── Navigation helpers ─────────────────────────────────────────────────
    public DateTime WeekStartDate { get; private set; }
    public DateTime? DayView { get; private set; }
    public int GridStartHour { get; private set; } = 7;
    public int GridEndHour { get; private set; } = 19;

    public DateTime PrevWeek => WeekStartDate.AddDays(-7);
    public DateTime NextWeek => WeekStartDate.AddDays(7);

    public DateTime PrevDay => (DayView ?? WeekStartDate).AddDays(-1);
    public DateTime NextDay => (DayView ?? WeekStartDate).AddDays(1);

    public DateTime PrevMonth
    {
        get
        {
            var first = new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1);
            return first.AddMonths(-1);
        }
    }

    public DateTime NextMonth
    {
        get
        {
            var first = new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1);
            return first.AddMonths(1);
        }
    }

    // ── OnGetAsync ─────────────────────────────────────────────────────────
    public async Task OnGetAsync(CancellationToken ct)
    {
        var roleName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        IsEmployeeView = string.Equals(roleName, RoleNames.Employee, StringComparison.OrdinalIgnoreCase);

        // Parse anchor date
        if (!string.IsNullOrEmpty(WeekStart) && DateTime.TryParse(WeekStart, out var parsedWeek))
            WeekStartDate = GetMondayOf(parsedWeek);
        else
            WeekStartDate = GetMondayOf(DateTime.UtcNow);

        if (View == "day" && !string.IsNullOrEmpty(DayDate) && DateTime.TryParse(DayDate, out var parsedDay))
            DayView = parsedDay;
        else if (View == "day")
            DayView = WeekStartDate;

        // ── Employee path — completely unchanged ───────────────────────────
        if (IsEmployeeView)
        {
            var empId = User.GetEmployeeId();
            if (!empId.HasValue)
            {
                ErrorMessage = "No employee profile associated with your account.";
                return;
            }

            var empWeekStart = GetMondayOf(DateTime.UtcNow);
            Board = await _kanbanManager.GetEmployeeWeekAsync(empId.Value, empWeekStart, ct);
            View = "week";
            ComputeGridHours();
            return;
        }

        // ── Admin/dispatcher path — new resource grid ──────────────────────
        var anchorDate = View switch
        {
            "day" => DayView ?? WeekStartDate,
            "month" => new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1),
            _ => WeekStartDate
        };

        ResourceGrid = await _kanbanManager.GetResourceGridAsync(anchorDate, View, ct);
        ComputeAdminGridHours();
    }

    // ── Handlers (unchanged) ───────────────────────────────────────────────
    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new
        {
            view = View,
            weekStart = WeekStart,
            dayDate = DayDate,
            filterEmployeeId = FilterEmployeeId
        });
    }

    public async Task<IActionResult> OnPostAddAssignmentAsync(int id, int employeeId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.AddAssignmentAsync(id, employeeId, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new
        {
            view = View,
            weekStart = WeekStart,
            dayDate = DayDate,
            filterEmployeeId = FilterEmployeeId
        });
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private void ComputeGridHours()
    {
        if (Board?.Days == null || !Board.Days.Any()) return;
        var allBookings = Board.Days.SelectMany(d => d.Bookings).ToList();
        if (!allBookings.Any()) return;
        GridStartHour = Math.Max(0, allBookings.Min(b => b.Hour) - 1);
        GridEndHour = Math.Min(24, allBookings.Max(b => b.Hour) + 2);
    }

    private void ComputeAdminGridHours()
    {
        if (ResourceGrid == null) return;

        var allBookings = ResourceGrid.Employees
            .SelectMany(e => e.Bookings)
            .Concat(ResourceGrid.Unassigned)
            .ToList();

        if (!allBookings.Any()) return;

        GridStartHour = Math.Max(6, allBookings.Min(b => b.Hour) - 1);
        GridEndHour = Math.Min(23, allBookings.Max(b => b.Hour) + 2);
    }

    private static DateTime GetMondayOf(DateTime date)
    {
        var d = date.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }
}