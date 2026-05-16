// ===== Clients.cshtml.cs =====
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesClients)]
public class ClientsModel : PageModel
{
    private readonly ClientManager _clientManager;
    public ClientsModel(ClientManager clientManager) { _clientManager = clientManager; }

    public List<ClientResponse> Clients { get; set; } = [];

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }
    [BindProperty] public CreateClientRequest NewClient { get; set; } = new();

    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync() => Clients = await _clientManager.GetAllAsync(Search, TypeFilter);

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!User.HasPermission(PermissionKeys.ClientsCreate))
            return Forbid();

        var result = await _clientManager.CreateAsync(NewClient);
        if (!result.Success)
        {
            ErrorMessage = result.Message;
            Clients = await _clientManager.GetAllAsync(Search, TypeFilter);
            return Page();
        }
        return RedirectToPage(new { search = Search, typeFilter = TypeFilter });
    }
}
