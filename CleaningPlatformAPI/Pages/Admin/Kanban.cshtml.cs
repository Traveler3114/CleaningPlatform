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

    public WeeklyBoardResponse Board { get; private set; } = null!;
    public bool IsEmployeeView { get; set; }

    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public string? WeekStart { get; set; }
    [BindProperty(SupportsGet = true)] public string? DayDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? MonthDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? FilterEmployeeId { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    // Computed navigation properties
    public DateTime WeekStartDate { get; private set; }
    public DateTime? DayView { get; private set; }
    public int GridStartHour { get; private set; } = 7;
    public int GridEndHour { get; private set; } = 19;

    // Navigation helpers (safe against uninitialized dates)
    public DateTime PrevWeek => WeekStartDate.AddDays(-7);
    public DateTime NextWeek => WeekStartDate.AddDays(7);

    public DateTime PrevDay
    {
        get
        {
            var baseDate = DayView ?? WeekStartDate;
            return baseDate.AddDays(-1);
        }
    }

    public DateTime NextDay
    {
        get
        {
            var baseDate = DayView ?? WeekStartDate;
            return baseDate.AddDays(1);
        }
    }

    public DateTime PrevMonth
    {
        get
        {
            if (WeekStartDate == default)
                return GetMondayOf(DateTime.UtcNow).AddMonths(-1);
            var first = new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1);
            return first.AddMonths(-1);
        }
    }

    public DateTime NextMonth
    {
        get
        {
            if (WeekStartDate == default)
                return GetMondayOf(DateTime.UtcNow).AddMonths(1);
            var first = new DateTime(WeekStartDate.Year, WeekStartDate.Month, 1);
            return first.AddMonths(1);
        }
    }

    // ----------------------------------------------------------------------
    //  OnGetAsync – fully implemented for both employee and admin
    // ----------------------------------------------------------------------
    public async Task OnGetAsync(CancellationToken ct)
    {
        // 1) Employee view (my schedule)
        if (IsEmployeeView)
        {
            var empId = User.GetEmployeeId();
            if (!empId.HasValue)
            {
                ErrorMessage = "No employee profile associated with your account.";
                return;
            }

            WeekStartDate = GetMondayOf(DateTime.UtcNow);
            Board = await _kanbanManager.GetEmployeeWeekAsync(empId.Value, WeekStartDate, ct);
            View = "week";                     // employee always sees week view
            ComputeGridHours();
            return;
        }

        // 2) Admin view – parse query parameters
        // WeekStart
        if (!string.IsNullOrEmpty(WeekStart) && DateTime.TryParse(WeekStart, out var parsedWeek))
            WeekStartDate = GetMondayOf(parsedWeek);
        else
            WeekStartDate = GetMondayOf(DateTime.UtcNow);

        // DayView (for day mode)
        if (View == "day" && !string.IsNullOrEmpty(DayDate) && DateTime.TryParse(DayDate, out var parsedDay))
            DayView = parsedDay;
        else if (View == "day")
            DayView = WeekStartDate;

        // Fetch board data – expects a method GetWeekAsync in KanbanManager
        Board = await _kanbanManager.GetWeekAsync(WeekStartDate, FilterEmployeeId, ct);

        // Adjust grid hours based on actual bookings
        ComputeGridHours();
    }

    // ----------------------------------------------------------------------
    //  Handlers
    // ----------------------------------------------------------------------
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

    // ----------------------------------------------------------------------
    //  Helpers
    // ----------------------------------------------------------------------
    private void ComputeGridHours()
    {
        if (Board?.Days == null || !Board.Days.Any())
        {
            // fallback defaults
            GridStartHour = 7;
            GridEndHour = 19;
            return;
        }

        var allBookings = Board.Days.SelectMany(d => d.Bookings).ToList();
        if (!allBookings.Any())
            return;

        var minHour = allBookings.Min(b => b.Hour);
        var maxHour = allBookings.Max(b => b.Hour);
        GridStartHour = Math.Max(0, minHour - 1);
        GridEndHour = Math.Min(24, maxHour + 2);
    }

    private static DateTime GetMondayOf(DateTime date)
    {
        var d = date.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }
}