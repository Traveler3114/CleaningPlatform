using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/assignments")]
public class AssignmentController : ControllerBase
{
    private readonly SopManager _sopManager;
    public AssignmentController(SopManager sopManager) => _sopManager = sopManager;

    [HttpGet("{assignmentId:int}/checklist")]
    public async Task<ActionResult<OperationResult<List<ChecklistResponseResponse>>>> GetChecklist(int assignmentId, CancellationToken ct)
    {
        var checklist = await _sopManager.GetChecklistForAssignmentAsync(assignmentId, ct);
        return Ok(OperationResult<List<ChecklistResponseResponse>>.Ok(checklist));
    }

    [HttpPost("{assignmentId:int}/checklist/{itemId:int}")]
    public async Task<ActionResult<OperationResult<ChecklistResponseResponse>>> CompleteItem(int assignmentId, int itemId, [FromBody] CompleteChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.CompleteChecklistItemAsync(assignmentId, itemId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}
