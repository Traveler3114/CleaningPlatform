using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/sops")]
public class SopController : ControllerBase
{
    private readonly SopManager _sopManager;
    public SopController(SopManager sopManager) { _sopManager = sopManager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.SopsView)]
    public async Task<ActionResult<List<SopTemplateResponse>>> Get(CancellationToken ct)
    {
        return Ok(await _sopManager.GetAllTemplatesAsync(ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsView)]
    public async Task<ActionResult<SopTemplateResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _sopManager.GetTemplateByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<SopTemplateResponse>> Create([FromBody] CreateSopTemplateRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.CreateTemplateAsync(request, ct));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<SopTemplateResponse>> Update(int id, [FromBody] CreateSopTemplateRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.UpdateTemplateAsync(id, request, ct));
    }

    [HttpPut("{id:int}/active")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<SopTemplateResponse>> ToggleActive(int id, [FromBody] ToggleSopActiveRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.ToggleActiveAsync(id, request.IsActive, ct));
    }

    [HttpPost("{id:int}/items")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<ChecklistItemResponse>> AddItem(int id, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.AddChecklistItemAsync(id, request, ct));
    }

    [HttpPut("items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<ChecklistItemResponse>> UpdateItem(int itemId, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.UpdateChecklistItemAsync(itemId, request, ct));
    }

    [HttpDelete("items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult> DeleteItem(int itemId, CancellationToken ct)
    {
        await _sopManager.DeleteChecklistItemAsync(itemId, ct);
        return NoContent();
    }
}