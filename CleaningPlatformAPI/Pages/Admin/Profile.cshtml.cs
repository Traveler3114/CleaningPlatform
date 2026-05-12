using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly EmployeeManager _employeeManager;
    private readonly BookingManager _bookingManager;
    private readonly AuthManager _authManager;

    public ProfileModel(EmployeeManager employeeManager, BookingManager bookingManager, AuthManager authManager)
    {
        _employeeManager = employeeManager;
        _bookingManager = bookingManager;
        _authManager = authManager;
    }

    public UserResponse? CurrentUser { get; set; }
    public List<BookingResponse> AssignedBookings { get; set; } = [];

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.GetEmployeeId();
        if (!userId.HasValue)
            return RedirectToPage("/Admin/Login");

        CurrentUser = await _employeeManager.GetByIdAsync(userId.Value);
        if (CurrentUser == null)
            return RedirectToPage("/Admin/Login");

        AssignedBookings = await _bookingManager.GetAssignedBookingsForEmployeeAsync(userId.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(string currentPassword, string newPassword)
    {
        var userId = User.GetEmployeeId();
        if (!userId.HasValue)
            return RedirectToPage("/Admin/Login");

        var result = await _authManager.ChangePasswordAsync(new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        }, userId.Value);

        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return RedirectToPage();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Admin/Login");
    }
}
