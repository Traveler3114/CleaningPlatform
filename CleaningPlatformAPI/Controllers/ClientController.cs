using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Models;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientController : ControllerBase
{
    private readonly ClientManager _clientManager;

    public ClientController(ClientManager clientManager) { _clientManager = clientManager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<Paginated<ClientResponse>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? type,
        CancellationToken ct)
    {
        return Ok(await _clientManager.GetAllAsync(pagination, type, ct));
    }

    [HttpGet("{id:int}", Name = "GetClientById")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<ClientResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _clientManager.GetByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ClientsCreate)]
    public async Task<ActionResult<ClientResponse>> Create(
        [FromBody] CreateClientRequest request, CancellationToken ct)
    {
        return Ok(await _clientManager.CreateAsync(request, ct));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<ActionResult<ClientResponse>> UpdateProfile(
        int id, [FromBody] UpdateClientProfileRequest request, CancellationToken ct)
    {
        return Ok(await _clientManager.UpdateProfileAsync(id, request, ct));
    }

    [HttpGet("{id:int}/bookings")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<Paginated<BookingResponse>>> GetBookings(
        int id, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        return Ok(await _clientManager.GetClientBookingsAsync(id, pagination, ct));
    }

    [HttpGet("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<List<SiteResponse>>> GetSites(int id, CancellationToken ct)
    {
        return Ok(await _clientManager.GetSitesAsync(id, ct));
    }

    [HttpPost("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<ActionResult<SiteResponse>> CreateSite(
        int id, [FromBody] UpsertSiteRequest request, CancellationToken ct)
    {
        return Ok(await _clientManager.CreateSiteAsync(id, request, ct));
    }

    [HttpPut("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<ActionResult<SiteResponse>> UpdateSite(
        int id, int siteId, [FromBody] UpsertSiteRequest request, CancellationToken ct)
    {
        return Ok(await _clientManager.UpdateSiteAsync(id, siteId, request, ct));
    }

    [HttpDelete("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.ClientsDelete)]
    public async Task<ActionResult<SiteResponse>> DeactivateSite(
        int id, int siteId, CancellationToken ct)
    {
        return Ok(await _clientManager.DeactivateSiteAsync(id, siteId, ct));
    }
}