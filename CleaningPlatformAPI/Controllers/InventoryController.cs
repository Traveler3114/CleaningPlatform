using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly InventoryManager _manager;
    public InventoryController(InventoryManager manager) { _manager = manager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryView)]
    public async Task<ActionResult<OperationResult<List<InventoryResponse>>>> GetAll(CancellationToken ct)
    {
        var items = await _manager.GetAllAsync(ct);
        return Ok(OperationResult<List<InventoryResponse>>.Ok(items));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.InventoryView)]
    public async Task<ActionResult<OperationResult<InventoryResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _manager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult<OperationResult<InventoryResponse>>> Create([FromBody] InventoryUpsertRequest request, CancellationToken ct)
    {
        var result = await _manager.CreateAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult<OperationResult<InventoryResponse>>> Update(int id, [FromBody] InventoryUpsertRequest request, CancellationToken ct)
    {
        var result = await _manager.UpdateAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id}/adjust-stock")]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult<OperationResult<string>>> AdjustStock(int id, [FromBody] InventoryAdjustmentRequest request, CancellationToken ct)
    {
        await _manager.AdjustStockAsync(id, request.AdjustmentAmount, ct);
        return Ok(OperationResult<string>.Ok("Stock adjusted."));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id, CancellationToken ct)
    {
        var result = await _manager.DeleteAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}
