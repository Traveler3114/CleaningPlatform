using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientController : ControllerBase
{
    private readonly ClientManager _clientManager;

    public ClientController(ClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<OperationResult<List<ClientResponse>>> GetAll([FromQuery] string? search, [FromQuery] string? type, CancellationToken ct)
    {
        return OperationResult<List<ClientResponse>>.Ok(await _clientManager.GetAllAsync(search, type, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<OperationResult<ClientResponse>> GetById(int id, CancellationToken ct)
    {
        var client = await _clientManager.GetByIdAsync(id, ct);
        return client is null
            ? OperationResult<ClientResponse>.Fail($"Client #{id} was not found.")
            : OperationResult<ClientResponse>.Ok(client);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ClientsCreate)]
    public async Task<OperationResult<ClientResponse>> Create([FromBody] CreateClientRequest request, CancellationToken ct)
    {
        return await _clientManager.CreateAsync(request, ct);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<OperationResult<ClientResponse>> UpdateProfile(int id, [FromBody] UpdateClientProfileRequest request, CancellationToken ct)
    {
        return await _clientManager.UpdateProfileAsync(id, request, ct);
    }

    [HttpGet("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<OperationResult<List<SiteResponse>>> GetSites(int id, CancellationToken ct)
    {
        return await _clientManager.GetSitesAsync(id, ct);
    }

    [HttpPost("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<OperationResult<SiteResponse>> CreateSite(int id, [FromBody] UpsertSiteRequest request, CancellationToken ct)
    {
        return await _clientManager.CreateSiteAsync(id, request, ct);
    }

    [HttpPut("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<OperationResult<SiteResponse>> UpdateSite(int id, int siteId, [FromBody] UpsertSiteRequest request, CancellationToken ct)
    {
        return await _clientManager.UpdateSiteAsync(id, siteId, request, ct);
    }

    [HttpDelete("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.ClientsDelete)]
    public async Task<OperationResult<SiteResponse>> DeactivateSite(int id, int siteId, CancellationToken ct)
    {
        return await _clientManager.DeactivateSiteAsync(id, siteId, ct);
    }
}
