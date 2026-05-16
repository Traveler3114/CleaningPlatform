using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesRoles)]
public class RolesModel : PageModel
{
    private readonly RoleManager _roleManager;

    public RolesModel(RoleManager roleManager) { _roleManager = roleManager; }

    public List<RoleResponse>             Roles                  { get; set; } = [];
    public List<AvailablePermissionResponse> AvailablePermissions { get; set; } = [];
    public Dictionary<string, string>     PermissionDisplayNames { get; set; } = new();
    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostCreateAsync(string name, List<string> permissions, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.RolesManage)) return Forbid();
        var result = await _roleManager.CreateRoleAsync(
            new CreateRoleRequest { Name = name, Permissions = permissions }, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string name, List<string> permissions, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.RolesManage)) return Forbid();
        var result = await _roleManager.UpdateRoleAsync(id,
            new UpdateRoleRequest { Name = name, Permissions = permissions }, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.RolesManage)) return Forbid();
        var result = await _roleManager.DeleteRoleAsync(id, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Roles                  = await _roleManager.GetAllRolesAsync(ct);
        AvailablePermissions   = _roleManager.GetAvailablePermissions();
        PermissionDisplayNames = AvailablePermissions.ToDictionary(p => p.Key, p => p.DisplayName);
    }
}
