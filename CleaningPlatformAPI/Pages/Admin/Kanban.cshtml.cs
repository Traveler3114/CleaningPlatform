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

    public KanbanBoardResponse Board { get; set; } = new(DateTime.UtcNow.Date, [], []);
    public bool IsEmployeeView { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;

    [BindProperty(SupportsGet = true)]
    public bool AllOpen { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        IsEmployeeView = string.Equals(role, RoleNames.Employee, StringComparison.OrdinalIgnoreCase);
        if (IsEmployeeView)
        {
            var employeeId = User.GetEmployeeId();
            Board = await _kanbanManager.GetBoardAsync(DateTime.UtcNow.Date, employeeId, ct);
            SelectedDate = DateTime.UtcNow.Date;
            return;
        }

        Board = AllOpen ? await _kanbanManager.GetPipelineAsync(ct) : await _kanbanManager.GetBoardAsync(SelectedDate, null, ct);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { selectedDate = SelectedDate.ToString("yyyy-MM-dd"), allOpen = AllOpen });
    }

    public async Task<IActionResult> OnPostAddAssignmentAsync(int id, int employeeId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingAssign))
            return Forbid();

        var result = await _bookingManager.AddAssignmentAsync(id, employeeId, ct);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { selectedDate = SelectedDate.ToString("yyyy-MM-dd"), allOpen = AllOpen });
    }
}
