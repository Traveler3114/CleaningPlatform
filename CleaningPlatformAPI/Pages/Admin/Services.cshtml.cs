using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize]
public class ServicesModel : PageModel
{
    private readonly ServiceCatalogManager _serviceCatalogManager;

    public ServicesModel(ServiceCatalogManager serviceCatalogManager)
    {
        _serviceCatalogManager = serviceCatalogManager;
    }

    public List<ServiceCatalogResponse> Services { get; set; } = [];

    [BindProperty]
    public ServiceCatalogUpsertRequest NewService { get; set; } = new();

    [BindProperty]
    public ServiceCatalogUpsertRequest EditService { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.HasPermission(PermissionKeys.ActionsServiceCatalogEdit) && !User.HasPermission(PermissionKeys.ActionsServiceCatalogManage))
            return Forbid();

        Services = await _serviceCatalogManager.GetAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!User.HasPermission(PermissionKeys.ActionsServiceCatalogManage))
            return Forbid();

        var result = await _serviceCatalogManager.CreateAsync(NewService);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id)
    {
        if (!User.HasPermission(PermissionKeys.ActionsServiceCatalogEdit))
            return Forbid();

        var result = await _serviceCatalogManager.UpdateAsync(id, EditService);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!User.HasPermission(PermissionKeys.ActionsServiceCatalogManage))
            return Forbid();

        var result = await _serviceCatalogManager.DeleteAsync(id);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }
}
