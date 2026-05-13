using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesSop)]
public class SopsModel : PageModel
{
    private readonly SopManager _sopManager;
    private readonly AppDbContext _db;

    public SopsModel(SopManager sopManager, AppDbContext db)
    {
        _sopManager = sopManager;
        _db = db;
    }

    public List<SopTemplateResponse> Templates { get; set; } = [];
    public List<ServiceCatalog> Services { get; set; } = [];

    [BindProperty]
    public CreateSopTemplateRequest Template { get; set; } = new();
    [BindProperty]
    public UpsertChecklistItemRequest ChecklistItem { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsSopManage)) return Forbid();
        var result = await _sopManager.CreateTemplateAsync(Template, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsSopManage)) return Forbid();
        var result = await _sopManager.UpdateTemplateAsync(id, Template, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsSopManage)) return Forbid();
        var result = await _sopManager.DeleteTemplateAsync(id, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddItemAsync(int templateId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsSopManage)) return Forbid();
        var result = await _sopManager.AddChecklistItemAsync(templateId, ChecklistItem, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateItemAsync(int itemId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsSopManage)) return Forbid();
        var result = await _sopManager.UpdateChecklistItemAsync(itemId, ChecklistItem, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteItemAsync(int itemId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsSopManage)) return Forbid();
        var result = await _sopManager.DeleteChecklistItemAsync(itemId, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Templates = await _sopManager.GetAllTemplatesAsync(ct);
        Services = await _db.ServiceCatalog.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(ct);
    }
}
