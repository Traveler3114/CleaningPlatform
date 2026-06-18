using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

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
    public async Task<ActionResult<List<InventoryResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await _manager.GetAllAsync(ct));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.InventoryView)]
    public async Task<ActionResult<InventoryResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _manager.GetByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult<InventoryResponse>> Create([FromBody] InventoryUpsertRequest request, CancellationToken ct)
    {
        return Ok(await _manager.CreateAsync(request, ct));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult<InventoryResponse>> Update(int id, [FromBody] InventoryUpsertRequest request, CancellationToken ct)
    {
        return Ok(await _manager.UpdateAsync(id, request, ct));
    }

    [HttpPost("{id}/adjust-stock")]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult> AdjustStock(int id, [FromBody] InventoryAdjustmentRequest request, CancellationToken ct)
    {
        await _manager.AdjustStockAsync(id, request.AdjustmentAmount, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.InventoryManage)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _manager.DeleteAsync(id, ct);
        return NoContent();
    }
}
