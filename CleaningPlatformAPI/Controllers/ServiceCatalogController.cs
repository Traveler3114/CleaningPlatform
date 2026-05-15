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

    public ServiceCatalogController(ServiceCatalogManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<OperationResult<List<ServiceCatalogResponse>>> Get(CancellationToken ct)
    {
        return OperationResult<List<ServiceCatalogResponse>>.Ok(await _manager.GetAllAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<OperationResult<ServiceCatalogResponse>> Post([FromBody] ServiceCatalogUpsertRequest request, CancellationToken ct)
    {
        return await _manager.CreateAsync(request, ct);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<OperationResult<ServiceCatalogResponse>> Put(int id, [FromBody] ServiceCatalogUpsertRequest request, CancellationToken ct)
    {
        return await _manager.UpdateAsync(id, request, ct);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ServicesManage)]
    public async Task<OperationResult<string>> Delete(int id, CancellationToken ct)
    {
        return await _manager.DeleteAsync(id, ct);
    }
}
