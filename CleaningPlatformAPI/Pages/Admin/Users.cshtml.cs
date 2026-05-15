using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesUsers)]
public class UsersModel : PageModel
{
    private readonly EmployeeManager _employeeManager;
    private readonly RoleManager _roleManager;
    private readonly AuthManager _authManager;

    public UsersModel(EmployeeManager employeeManager, RoleManager roleManager, AuthManager authManager)
    {
        _employeeManager = employeeManager;
        _roleManager = roleManager;
        _authManager = authManager;
    }

    public List<UserResponse> Users { get; set; } = [];
    public List<RoleResponse> AvailableRoles { get; set; } = [];

    [BindProperty]
    public CreateUserRequest NewUser { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!User.HasPermission(PermissionKeys.UsersCreate))
            return Forbid();

        var result = await _authManager.RegisterAsync(NewUser);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        if (!User.HasPermission(PermissionKeys.UsersEdit))
            return Forbid();

        var currentUserId = User.GetEmployeeId();
        if (!currentUserId.HasValue)
        {
            ErrorMessage = "Invalid user context.";
            return RedirectToPage();
        }

        var result = await _employeeManager.ToggleActiveAsync(id, currentUserId.Value);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(int userId, string newPassword)
    {
        if (!User.HasPermission(PermissionKeys.UsersEdit))
            return Forbid();

        var result = await _authManager.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = userId,
            NewPassword = newPassword
        });

        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Users = await _employeeManager.GetAllUsersAsync();
        AvailableRoles = await _roleManager.GetAllRolesAsync();
    }
}
