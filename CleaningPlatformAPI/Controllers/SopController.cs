using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/sops")]
public class SopController : ControllerBase
{
    private readonly SopManager _sopManager;
    public SopController(SopManager sopManager) => _sopManager = sopManager;

    [HttpGet]
    [Authorize(Policy = PermissionKeys.SopsView)]
    public async Task<ActionResult<OperationResult<List<SopTemplateResponse>>>> Get(CancellationToken ct)
    {
        var templates = await _sopManager.GetAllTemplatesAsync(ct);
        return Ok(OperationResult<List<SopTemplateResponse>>.Ok(templates));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsView)]
    public async Task<ActionResult<OperationResult<SopTemplateResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _sopManager.GetTemplateByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<SopTemplateResponse>>> Create([FromBody] CreateSopTemplateRequest request, CancellationToken ct)
    {
        var result = await _sopManager.CreateTemplateAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<SopTemplateResponse>>> Update(int id, [FromBody] CreateSopTemplateRequest request, CancellationToken ct)
    {
        var result = await _sopManager.UpdateTemplateAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id, CancellationToken ct)
    {
        var result = await _sopManager.DeleteTemplateAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id:int}/items")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<ChecklistItemResponse>>> AddItem(int id, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.AddChecklistItemAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<ChecklistItemResponse>>> UpdateItem(int itemId, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.UpdateChecklistItemAsync(itemId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<string>>> DeleteItem(int itemId, CancellationToken ct)
    {
        var result = await _sopManager.DeleteChecklistItemAsync(itemId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}