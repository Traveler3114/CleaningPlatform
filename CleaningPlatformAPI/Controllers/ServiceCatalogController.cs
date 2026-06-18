using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServiceCatalogController : ControllerBase
{
    private readonly ServiceCatalogManager _manager;
    public ServiceCatalogController(ServiceCatalogManager manager) { _manager = manager; }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ServiceCatalogResponse>>> Get(CancellationToken ct)
    {
        return Ok(await _manager.GetAllAsync(ct));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceCatalogResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _manager.GetByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<ServiceCatalogResponse>> Post([FromBody] ServiceCatalogUpsertRequest request, CancellationToken ct)
    {
        return Ok(await _manager.CreateAsync(request, ct));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<ServiceCatalogResponse>> Put(int id, [FromBody] ServiceCatalogUpsertRequest request, CancellationToken ct)
    {
        return Ok(await _manager.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _manager.DeleteAsync(id, ct);
        return NoContent();
    }

    // ── Inventory Requirements ─────────────────────────────────

    [HttpGet("{serviceId}/requirements")]
    [Authorize(Policy = PermissionKeys.ServicesView)]
    public async Task<ActionResult<List<RequirementResponse>>> GetRequirements(int serviceId, CancellationToken ct)
    {
        return Ok(await _manager.GetRequirementsAsync(serviceId, ct));
    }

    [HttpPost("{serviceId}/requirements")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<RequirementResponse>> AddRequirement(int serviceId, [FromBody] RequirementUpsertRequest request, CancellationToken ct)
    {
        return Ok(await _manager.AddRequirementAsync(serviceId, request, ct));
    }

    [HttpPut("{serviceId}/requirements/{inventoryId}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<RequirementResponse>> UpdateRequirement(int serviceId, int inventoryId, [FromBody] RequirementUpsertRequest request, CancellationToken ct)
    {
        return Ok(await _manager.UpdateRequirementAsync(serviceId, inventoryId, request, ct));
    }

    [HttpDelete("{serviceId}/requirements/{inventoryId}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult> RemoveRequirement(int serviceId, int inventoryId, CancellationToken ct)
    {
        await _manager.RemoveRequirementAsync(serviceId, inventoryId, ct);
        return NoContent();
    }
}
