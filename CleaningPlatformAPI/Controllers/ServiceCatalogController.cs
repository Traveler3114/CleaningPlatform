using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<ServiceCatalogDto>>> Get()
    {
        var services = await _manager.GetAllAsync();
        return OperationResult<List<ServiceCatalogDto>>.Ok(services);
    }

    [HttpPost]
    [Authorize(Policy = "actions.serviceCatalog.manage")]
    public async Task<OperationResult<ServiceCatalogDto>> Post([FromBody] ServiceCatalogUpsertDto dto)
    {
        return await _manager.CreateAsync(dto);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "actions.serviceCatalog.edit")]
    public async Task<OperationResult<ServiceCatalogDto>> Put(int id, [FromBody] ServiceCatalogUpsertDto dto)
    {
        return await _manager.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "actions.serviceCatalog.manage")]
    public async Task<OperationResult<string>> Delete(int id)
    {
        return await _manager.DeleteAsync(id);
    }
}