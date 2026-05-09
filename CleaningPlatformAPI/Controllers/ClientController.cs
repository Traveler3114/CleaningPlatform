// CleaningPlatformAPI/Controllers/ClientController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<ClientDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? type)
    {
        var clients = await _clientManager.GetAllAsync(search, type);
        return OperationResult<List<ClientDto>>.Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<OperationResult<ClientProfileDto>> GetById(int id)
    {
        var client = await _clientManager.GetByIdAsync(id);
        return client is null
            ? OperationResult<ClientProfileDto>.Fail("Client not found.")
            : OperationResult<ClientProfileDto>.Ok(client);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<OperationResult<ClientDto>> Create([FromBody] CreateClientDto dto)
    {
        return await _clientManager.CreateAsync(dto);
    }

    [HttpPut("{id:int}/type")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<OperationResult<ClientDto>> UpdateType(int id, [FromBody] UpdateTypeRequest request)
    {
        return await _clientManager.UpdateTypeAsync(id, request.NewType);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<OperationResult<ClientProfileDto>> UpdateProfile(int id, [FromBody] UpdateClientProfileDto dto)
    {
        return await _clientManager.UpdateProfileAsync(id, dto);
    }

    [HttpGet("{id:int}/sites")]
    public async Task<OperationResult<List<SiteDto>>> GetSites(int id)
    {
        return await _clientManager.GetSitesAsync(id);
    }

    [HttpPost("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<OperationResult<SiteDto>> CreateSite(int id, [FromBody] UpsertSiteDto dto)
    {
        return await _clientManager.CreateSiteAsync(id, dto);
    }

    [HttpPut("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<OperationResult<SiteDto>> UpdateSite(int id, int siteId, [FromBody] UpsertSiteDto dto)
    {
        return await _clientManager.UpdateSiteAsync(id, siteId, dto);
    }

    [HttpDelete("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<OperationResult<SiteDto>> DeactivateSite(int id, int siteId)
    {
        return await _clientManager.DeactivateSiteAsync(id, siteId);
    }
}

public class UpdateTypeRequest
{
    public string NewType { get; set; } = string.Empty;
}