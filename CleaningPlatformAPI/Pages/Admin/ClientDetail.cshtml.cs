using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesClients)]
public class ClientDetailModel : PageModel
{
    private readonly ClientManager _clientManager;

    public ClientDetailModel(ClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public ClientResponse? Client { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public UpdateClientProfileRequest Profile { get; set; } = new();

    [BindProperty]
    public string? ContactsJson { get; set; }

    [BindProperty]
    public UpsertSiteRequest NewSite { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = await _clientManager.GetByIdAsync(Id);
        if (client == null) return NotFound();

        Client = client;
        Profile = new UpdateClientProfileRequest
        {
            ClientName = client.ClientName,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            Notes = client.Notes,
            Contacts = client.Contacts.Select(c => new UpsertContactRequest
            {
                Id = c.Id,
                ContactName = c.ContactName,
                Role = c.Role,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                IsPrimary = c.IsPrimary,
                IsActive = c.IsActive
            }).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        if (!string.IsNullOrWhiteSpace(ContactsJson))
        {
            var parsed = JsonSerializer.Deserialize<List<UpsertContactRequest>>(ContactsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Profile.Contacts = parsed ?? [];
        }

        var result = await _clientManager.UpdateProfileAsync(Id, Profile);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { id = Id });
    }


    public async Task<IActionResult> OnPostAddSiteAsync()
    {
        var result = await _clientManager.CreateSiteAsync(Id, NewSite);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostUpdateSiteAsync(int siteId, UpsertSiteRequest site)
    {
        var result = await _clientManager.UpdateSiteAsync(Id, siteId, site);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeactivateSiteAsync(int siteId)
    {
        var result = await _clientManager.DeactivateSiteAsync(Id, siteId);
        if (!result.Success)
            ErrorMessage = result.Message;

        return RedirectToPage(new { id = Id });
    }
}
