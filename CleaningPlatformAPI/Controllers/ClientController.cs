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
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? type)
    {
        var clients = await _clientManager.GetAllAsync(search, type);
        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _clientManager.GetByIdAsync(id);
        if (client == null)
            return NotFound("Client not found.");
        return Ok(client);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
    {
        var result = await _clientManager.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/type")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> UpdateType(int id, [FromBody] UpdateTypeRequest request)
    {
        var result = await _clientManager.UpdateTypeAsync(id, request.NewType);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateClientProfileDto dto)
    {
        var result = await _clientManager.UpdateProfileAsync(id, dto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}/sites")]
    public async Task<IActionResult> GetSites(int id)
    {
        var result = await _clientManager.GetSitesAsync(id);
        if (!result.Success)
            return NotFound(result.Message);
        return Ok(result.Data);
    }

    [HttpPost("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> CreateSite(int id, [FromBody] UpsertSiteDto dto)
    {
        var result = await _clientManager.CreateSiteAsync(id, dto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> UpdateSite(int id, int siteId, [FromBody] UpsertSiteDto dto)
    {
        var result = await _clientManager.UpdateSiteAsync(id, siteId, dto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> DeactivateSite(int id, int siteId)
    {
        var result = await _clientManager.DeactivateSiteAsync(id, siteId);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }
}

// Simple request DTO (can be in the same file or a separate one)
public class UpdateTypeRequest
{
    public string NewType { get; set; } = string.Empty;
}
