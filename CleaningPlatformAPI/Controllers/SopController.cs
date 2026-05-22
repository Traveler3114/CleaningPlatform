using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Authorize]
public class SopController : ControllerBase
{
    private readonly SopManager _sopManager;
    public SopController(SopManager sopManager) => _sopManager = sopManager;

    [HttpGet("api/sops")]
    [Authorize(Policy = PermissionKeys.SopsView)]
    public async Task<ActionResult<OperationResult<List<SopTemplateResponse>>>> Get(CancellationToken ct)
    {
        var templates = await _sopManager.GetAllTemplatesAsync(ct);
        return Ok(OperationResult<List<SopTemplateResponse>>.Ok(templates));
    }

    [HttpGet("api/sops/{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsView)]
    public async Task<ActionResult<OperationResult<SopTemplateResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _sopManager.GetTemplateByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("api/sops")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<SopTemplateResponse>>> Create([FromBody] CreateSopTemplateRequest request, CancellationToken ct)
    {
        var result = await _sopManager.CreateTemplateAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("api/sops/{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<SopTemplateResponse>>> Update(int id, [FromBody] CreateSopTemplateRequest request, CancellationToken ct)
    {
        var result = await _sopManager.UpdateTemplateAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("api/sops/{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id, CancellationToken ct)
    {
        var result = await _sopManager.DeleteTemplateAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("api/sops/{id:int}/items")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<ChecklistItemResponse>>> AddItem(int id, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.AddChecklistItemAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("api/sops/items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<ChecklistItemResponse>>> UpdateItem(int itemId, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.UpdateChecklistItemAsync(itemId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("api/sops/items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public async Task<ActionResult<OperationResult<string>>> DeleteItem(int itemId, CancellationToken ct)
    {
        var result = await _sopManager.DeleteChecklistItemAsync(itemId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("api/bookings/{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<List<BookingSopAssignmentResponse>>>> GetBookingSops(int id, CancellationToken ct)
    {
        var assignments = await _sopManager.GetBookingSopsAsync(id, ct);
        return Ok(OperationResult<List<BookingSopAssignmentResponse>>.Ok(assignments));
    }

    [HttpPost("api/bookings/{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingSopAssignmentResponse>>> AssignSop(int id, [FromBody] AssignSopRequest request, CancellationToken ct)
    {
        var result = await _sopManager.AssignSopToBookingAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("api/assignments/{assignmentId:int}/checklist")]
    public async Task<ActionResult<OperationResult<List<ChecklistResponseResponse>>>> GetChecklist(int assignmentId, CancellationToken ct)
    {
        var checklist = await _sopManager.GetChecklistForAssignmentAsync(assignmentId, ct);
        return Ok(OperationResult<List<ChecklistResponseResponse>>.Ok(checklist));
    }

    [HttpPost("api/assignments/{assignmentId:int}/checklist/{itemId:int}")]
    public async Task<ActionResult<OperationResult<ChecklistResponseResponse>>> CompleteItem(int assignmentId, int itemId, [FromBody] CompleteChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.CompleteChecklistItemAsync(assignmentId, itemId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}