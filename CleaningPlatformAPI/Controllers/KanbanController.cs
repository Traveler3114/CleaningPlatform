using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/kanban")]
[Authorize(Policy = PermissionKeys.PagesKanban)]
public class KanbanController : ControllerBase
{
    private readonly KanbanManager _kanbanManager;

    public KanbanController(KanbanManager kanbanManager) { _kanbanManager = kanbanManager; }

    [HttpGet]
    public async Task<ActionResult<KanbanBoardResponse>> Get([FromQuery] DateTime? date, [FromQuery] int? employeeId, CancellationToken ct)
    {
        return Ok(await _kanbanManager.GetBoardAsync(date ?? DateTime.UtcNow.Date, employeeId, ct));
    }

    [HttpGet("pipeline")]
    public async Task<ActionResult<KanbanBoardResponse>> Pipeline(CancellationToken ct)
    {
        return Ok(await _kanbanManager.GetPipelineAsync(ct));
    }

    [HttpGet("resourcegrid")]
    public async Task<ActionResult<ResourceGridResponse>> GetResourceGrid(
        [FromQuery] DateTime anchorDate,
        [FromQuery] string view,
        CancellationToken ct)
    {
        return Ok(await _kanbanManager.GetResourceGridAsync(anchorDate, view, ct));
    }

    [HttpGet("equipment-warnings")]
    public async Task<ActionResult<List<EquipmentWarningResponse>>> GetEquipmentWarnings(
        [FromQuery] DateTime date,
        CancellationToken ct)
    {
        return Ok(await _kanbanManager.GetEquipmentWarningsAsync(date, ct));
    }

    [HttpGet("employee-week")]
    [Authorize(Policy = PermissionKeys.PagesKanban)] // or use a more specific policy if needed
    public async Task<ActionResult<WeeklyBoardResponse>> GetEmployeeWeek(
        [FromQuery] DateTime weekStart,
        CancellationToken ct)
    {
        var userId = User.GetEmployeeId();
        if (userId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");

        return Ok(await _kanbanManager.GetEmployeeWeekAsync(userId.Value, weekStart, ct));
    }
}

