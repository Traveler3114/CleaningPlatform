using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

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
    public async Task<ActionResult<List<WeeklyScheduleResponse>>> Get(CancellationToken ct)
    {
        return Ok(await _manager.GetScheduleAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<WeeklyScheduleResponse>> Post([FromBody] WeeklyScheduleRequest request, CancellationToken ct)
    {
        return Ok(await _manager.CreateDayAsync(request, ct));
    }

    [HttpPut("{dayOfWeek}")]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<WeeklyScheduleResponse>> Put(int dayOfWeek, [FromBody] UpdateWeeklyScheduleRequest request, CancellationToken ct)
    {
        return Ok(await _manager.UpdateDayAsync(dayOfWeek, request, ct));
    }

    [HttpDelete("{dayOfWeek}")]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult> Delete(int dayOfWeek, CancellationToken ct)
    {
        await _manager.DeleteDayAsync(dayOfWeek, ct);
        return NoContent();
    }
}