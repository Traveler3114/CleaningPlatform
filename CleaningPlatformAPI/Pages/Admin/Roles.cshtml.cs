using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesRoles)]
public class RolesModel : PageModel
{
    private readonly RoleManager _roleManager;
    private static readonly Dictionary<string, string> LegacyPermissionDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["clients.view"] = "View Clients",
        ["clients.manage"] = "Manage Clients",
        ["sites.view"] = "View Sites",
        ["sites.manage"] = "Manage Sites",
        ["bookings.view"] = "View Bookings",
        ["bookings.manage"] = "Manage Bookings",
        ["invoices.view"] = "View Invoices",
        ["invoices.manage"] = "Manage Invoices",
        ["reports.view"] = "View Reports"
    };

    public RolesModel(RoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    public List<RoleDto> Roles { get; set; } = [];
    public List<AvailablePermissionDto> AvailablePermissions { get; set; } = [];
    public Dictionary<string, string> PermissionDisplayNames { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync(string name, List<string> permissions)
    {
        if (!User.HasPermission(PermissionKeys.ActionsRoleManage))
            return Forbid();

        var result = await _roleManager.CreateRoleAsync(new CreateRoleDto { Name = name, Permissions = permissions });
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string name, List<string> permissions)
    {
        if (!User.HasPermission(PermissionKeys.ActionsRoleManage))
            return Forbid();

        var result = await _roleManager.UpdateRoleAsync(id, new UpdateRoleDto { Name = name, Permissions = permissions });
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!User.HasPermission(PermissionKeys.ActionsRoleManage))
            return Forbid();

        var result = await _roleManager.DeleteRoleAsync(id);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Roles = await _roleManager.GetAllRolesAsync();
        AvailablePermissions = _roleManager.GetAvailablePermissions();
        PermissionDisplayNames = AvailablePermissions.ToDictionary(p => p.Key, p => p.DisplayName);
        foreach (var legacy in LegacyPermissionDisplayNames)
        {
            PermissionDisplayNames.TryAdd(legacy.Key, legacy.Value);
        }
    }
}
