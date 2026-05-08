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
    public async Task<ActionResult<OperationResult<ServiceCatalogDto>>> Post([FromBody] ServiceCatalogUpsertDto dto)
    {
        var result = await _manager.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "actions.serviceCatalog.edit")]
    public async Task<ActionResult<OperationResult<ServiceCatalogDto>>> Put(int id, [FromBody] ServiceCatalogUpsertDto dto)
    {
        var result = await _manager.UpdateAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "actions.serviceCatalog.manage")]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id)
    {
        var result = await _manager.DeleteAsync(id);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
