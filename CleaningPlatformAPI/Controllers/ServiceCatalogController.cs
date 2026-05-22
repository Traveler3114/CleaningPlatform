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
}