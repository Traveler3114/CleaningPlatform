using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesClients)]
public class ClientsModel : PageModel
{
    private readonly ClientManager _clientManager;

    public ClientsModel(ClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public List<ClientDto> Clients { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    [BindProperty]
    public CreateClientDto NewClient { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Clients = await _clientManager.GetAllAsync(Search, TypeFilter);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
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
