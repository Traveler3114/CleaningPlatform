using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

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
    public async Task<ActionResult<OperationResult<PagedResult<ClientResponse>>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? type,
        CancellationToken ct)
    {
        var paged = await _clientManager.GetAllAsync(pagination, type, ct);
        return Ok(OperationResult<PagedResult<ClientResponse>>.Ok(paged));
    }

    [HttpGet("{id:int}", Name = "GetClientById")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<OperationResult<ClientResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _clientManager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ClientsCreate)]
    public async Task<ActionResult<OperationResult<ClientResponse>>> Create(
        [FromBody] CreateClientRequest request, CancellationToken ct)
    {
        var result = await _clientManager.CreateAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<ActionResult<OperationResult<ClientResponse>>> UpdateProfile(
        int id, [FromBody] UpdateClientProfileRequest request, CancellationToken ct)
    {
        var result = await _clientManager.UpdateProfileAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("{id:int}/bookings")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<OperationResult<PagedResult<BookingResponse>>>> GetBookings(
        int id, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _clientManager.GetClientBookingsAsync(id, pagination, ct);
        return Ok(OperationResult<PagedResult<BookingResponse>>.Ok(result));
    }

    [HttpGet("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.ClientsView)]
    public async Task<ActionResult<OperationResult<List<SiteResponse>>>> GetSites(int id, CancellationToken ct)
    {
        var result = await _clientManager.GetSitesAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id:int}/sites")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<ActionResult<OperationResult<SiteResponse>>> CreateSite(
        int id, [FromBody] UpsertSiteRequest request, CancellationToken ct)
    {
        var result = await _clientManager.CreateSiteAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.ClientsEdit)]
    public async Task<ActionResult<OperationResult<SiteResponse>>> UpdateSite(
        int id, int siteId, [FromBody] UpsertSiteRequest request, CancellationToken ct)
    {
        var result = await _clientManager.UpdateSiteAsync(id, siteId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id:int}/sites/{siteId:int}")]
    [Authorize(Policy = PermissionKeys.ClientsDelete)]
    public async Task<ActionResult<OperationResult<SiteResponse>>> DeactivateSite(
        int id, int siteId, CancellationToken ct)
    {
        var result = await _clientManager.DeactivateSiteAsync(id, siteId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}