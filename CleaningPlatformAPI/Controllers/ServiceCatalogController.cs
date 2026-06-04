using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

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
    public async Task<ActionResult<OperationResult<List<ServiceCatalogResponse>>>> Get(CancellationToken ct)
    {
        var services = await _manager.GetAllAsync(ct);
        return Ok(OperationResult<List<ServiceCatalogResponse>>.Ok(services));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResult<ServiceCatalogResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _manager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<OperationResult<ServiceCatalogResponse>>> Post([FromBody] ServiceCatalogUpsertRequest request, CancellationToken ct)
    {
        var result = await _manager.CreateAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<OperationResult<ServiceCatalogResponse>>> Put(int id, [FromBody] ServiceCatalogUpsertRequest request, CancellationToken ct)
    {
        var result = await _manager.UpdateAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id, CancellationToken ct)
    {
        var result = await _manager.DeleteAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    // ── Inventory Requirements ─────────────────────────────────

    [HttpGet("{serviceId}/requirements")]
    [Authorize(Policy = PermissionKeys.ServicesView)]
    public async Task<ActionResult<OperationResult<List<RequirementResponse>>>> GetRequirements(int serviceId, CancellationToken ct)
    {
        var requirements = await _manager.GetRequirementsAsync(serviceId, ct);
        return Ok(OperationResult<List<RequirementResponse>>.Ok(requirements));
    }

    [HttpPost("{serviceId}/requirements")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<OperationResult<RequirementResponse>>> AddRequirement(int serviceId, [FromBody] RequirementUpsertRequest request, CancellationToken ct)
    {
        var result = await _manager.AddRequirementAsync(serviceId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{serviceId}/requirements/{inventoryId}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<OperationResult<RequirementResponse>>> UpdateRequirement(int serviceId, int inventoryId, [FromBody] RequirementUpsertRequest request, CancellationToken ct)
    {
        var result = await _manager.UpdateRequirementAsync(serviceId, inventoryId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{serviceId}/requirements/{inventoryId}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<ActionResult<OperationResult<string>>> RemoveRequirement(int serviceId, int inventoryId, CancellationToken ct)
    {
        var result = await _manager.RemoveRequirementAsync(serviceId, inventoryId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}
