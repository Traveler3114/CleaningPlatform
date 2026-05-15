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
    public async Task<OperationResult<List<SopTemplateResponse>>> Get(CancellationToken ct) => OperationResult<List<SopTemplateResponse>>.Ok(await _sopManager.GetAllTemplatesAsync(ct));

    [HttpGet("api/sops/{id:int}")]
    public async Task<OperationResult<SopTemplateResponse>> GetById(int id, CancellationToken ct)
    {
        var sop = await _sopManager.GetTemplateByIdAsync(id, ct);
        return sop is null ? OperationResult<SopTemplateResponse>.Fail("SOP template not found.") : OperationResult<SopTemplateResponse>.Ok(sop);
    }

    [HttpPost("api/sops")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public Task<OperationResult<SopTemplateResponse>> Create([FromBody] CreateSopTemplateRequest request, CancellationToken ct) => _sopManager.CreateTemplateAsync(request, ct);

    [HttpPut("api/sops/{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public Task<OperationResult<SopTemplateResponse>> Update(int id, [FromBody] CreateSopTemplateRequest request, CancellationToken ct) => _sopManager.UpdateTemplateAsync(id, request, ct);

    [HttpDelete("api/sops/{id:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public Task<OperationResult<string>> Delete(int id, CancellationToken ct) => _sopManager.DeleteTemplateAsync(id, ct);

    [HttpPost("api/sops/{id:int}/items")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public Task<OperationResult<ChecklistItemResponse>> AddItem(int id, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct) => _sopManager.AddChecklistItemAsync(id, request, ct);

    [HttpPut("api/sops/items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public Task<OperationResult<ChecklistItemResponse>> UpdateItem(int itemId, [FromBody] UpsertChecklistItemRequest request, CancellationToken ct) => _sopManager.UpdateChecklistItemAsync(itemId, request, ct);

    [HttpDelete("api/sops/items/{itemId:int}")]
    [Authorize(Policy = PermissionKeys.SopsManage)]
    public Task<OperationResult<string>> DeleteItem(int itemId, CancellationToken ct) => _sopManager.DeleteChecklistItemAsync(itemId, ct);

    [HttpGet("api/bookings/{id:int}/sops")]
    public async Task<OperationResult<List<BookingSopAssignmentResponse>>> GetBookingSops(int id, CancellationToken ct) => OperationResult<List<BookingSopAssignmentResponse>>.Ok(await _sopManager.GetBookingSopsAsync(id, ct));

    [HttpPost("api/bookings/{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public Task<OperationResult<BookingSopAssignmentResponse>> AssignSop(int id, [FromBody] AssignSopRequest request, CancellationToken ct) => _sopManager.AssignSopToBookingAsync(id, request, ct);

    [HttpGet("api/assignments/{assignmentId:int}/checklist")]
    public async Task<OperationResult<List<ChecklistResponseResponse>>> GetChecklist(int assignmentId, CancellationToken ct) => OperationResult<List<ChecklistResponseResponse>>.Ok(await _sopManager.GetChecklistForAssignmentAsync(assignmentId, ct));

    [HttpPost("api/assignments/{assignmentId:int}/checklist/{itemId:int}")]
    public Task<OperationResult<ChecklistResponseResponse>> CompleteItem(int assignmentId, int itemId, [FromBody] CompleteChecklistItemRequest request, CancellationToken ct) => _sopManager.CompleteChecklistItemAsync(assignmentId, itemId, request, ct);
}
