using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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

    public ClientProfileDto? Client { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public UpdateClientProfileDto Profile { get; set; } = new();

    [BindProperty]
    public string? ContactsJson { get; set; }

    [BindProperty]
    public UpsertSiteDto NewSite { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = await _clientManager.GetByIdAsync(Id);
        if (client == null) return NotFound();

        Client = client;
        Profile = new UpdateClientProfileDto
        {
            ClientName = client.ClientName,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            Notes = client.Notes,
            Contacts = client.Contacts.Select(c => new UpsertContactDto
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
            var parsed = JsonSerializer.Deserialize<List<UpsertContactDto>>(ContactsJson, new JsonSerializerOptions
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

    public async Task<IActionResult> OnPostUpdateSiteAsync(int siteId, UpsertSiteDto site)
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
