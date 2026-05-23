using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/kanban")]
[Authorize(Policy = PermissionKeys.PagesKanban)]
public class KanbanController : ControllerBase
{
    private readonly KanbanManager _kanbanManager;

    public KanbanController(KanbanManager kanbanManager) { _kanbanManager = kanbanManager; }

    [HttpGet]
    public async Task<ActionResult<OperationResult<KanbanBoardResponse>>> Get([FromQuery] DateTime? date, [FromQuery] int? employeeId, CancellationToken ct)
    {
        var board = await _kanbanManager.GetBoardAsync(date ?? DateTime.UtcNow.Date, employeeId, ct);
        return Ok(OperationResult<KanbanBoardResponse>.Ok(board));
    }

    [HttpGet("pipeline")]
    public async Task<ActionResult<OperationResult<KanbanBoardResponse>>> Pipeline(CancellationToken ct)
    {
        var pipeline = await _kanbanManager.GetPipelineAsync(ct);
        return Ok(OperationResult<KanbanBoardResponse>.Ok(pipeline));
    }

    [HttpGet("resourcegrid")]
    public async Task<ActionResult<OperationResult<ResourceGridResponse>>> GetResourceGrid(
        [FromQuery] DateTime anchorDate,
        [FromQuery] string view,
        CancellationToken ct)
    {
        var result = await _kanbanManager.GetResourceGridAsync(anchorDate, view, ct);
        return Ok(OperationResult<ResourceGridResponse>.Ok(result));
    }

    [HttpGet("employee-week")]
    [Authorize(Policy = PermissionKeys.PagesKanban)] // or use a more specific policy if needed
    public async Task<ActionResult<OperationResult<WeeklyBoardResponse>>> GetEmployeeWeek(
        [FromQuery] DateTime weekStart,
        CancellationToken ct)
    {
        var userId = User.GetEmployeeId();
        if (userId == null)
            return Unauthorized(OperationResult<WeeklyBoardResponse>.Fail("Invalid token."));

        var result = await _kanbanManager.GetEmployeeWeekAsync(userId.Value, weekStart, ct);
        return Ok(OperationResult<WeeklyBoardResponse>.Ok(result));
    }
}

