using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/schedule")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly ScheduleManager _manager;
    public ScheduleController(ScheduleManager manager) { _manager = manager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.ScheduleView)]
    public async Task<ActionResult<OperationResult<List<WeeklyScheduleResponse>>>> Get(CancellationToken ct)
    {
        var schedule = await _manager.GetScheduleAsync(ct);
        return Ok(OperationResult<List<WeeklyScheduleResponse>>.Ok(schedule));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<OperationResult<WeeklyScheduleResponse>>> Post([FromBody] WeeklyScheduleRequest request, CancellationToken ct)
    {
        var result = await _manager.CreateDayAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{dayOfWeek}")]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<OperationResult<WeeklyScheduleResponse>>> Put(int dayOfWeek, [FromBody] UpdateWeeklyScheduleRequest request, CancellationToken ct)
    {
        var result = await _manager.UpdateDayAsync(dayOfWeek, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{dayOfWeek}")]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<OperationResult<bool>>> Delete(int dayOfWeek, CancellationToken ct)
    {
        var result = await _manager.DeleteDayAsync(dayOfWeek, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}