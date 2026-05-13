using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/kanban")]
[Authorize(Policy = PermissionKeys.PagesKanban)]
public class KanbanController : ControllerBase
{
    private readonly KanbanManager _kanbanManager;

    public KanbanController(KanbanManager kanbanManager) => _kanbanManager = kanbanManager;

    [HttpGet]
    public async Task<OperationResult<KanbanBoardResponse>> Get([FromQuery] DateTime? date, [FromQuery] int? employeeId, CancellationToken ct) =>
        OperationResult<KanbanBoardResponse>.Ok(await _kanbanManager.GetBoardAsync(date ?? DateTime.UtcNow.Date, employeeId, ct));

    [HttpGet("pipeline")]
    public async Task<OperationResult<KanbanBoardResponse>> Pipeline(CancellationToken ct) =>
        OperationResult<KanbanBoardResponse>.Ok(await _kanbanManager.GetPipelineAsync(ct));
}
