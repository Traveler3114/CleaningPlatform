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

    public ScheduleController(ScheduleManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<OperationResult<List<WeeklyScheduleResponse>>> Get(CancellationToken ct)
    {
        return OperationResult<List<WeeklyScheduleResponse>>.Ok(await _manager.GetScheduleAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ActionsScheduleEdit)]
    public async Task<OperationResult<WeeklyScheduleResponse>> Post([FromBody] WeeklyScheduleRequest request, CancellationToken ct)
    {
        return await _manager.CreateDayAsync(request, ct);
    }

    [HttpPut("{dayOfWeek}")]
    [Authorize(Policy = PermissionKeys.ActionsScheduleEdit)]
    public async Task<OperationResult<WeeklyScheduleResponse>> Put(int dayOfWeek, [FromBody] UpdateWeeklyScheduleRequest request, CancellationToken ct)
    {
        return await _manager.UpdateDayAsync(dayOfWeek, request, ct);
    }

    [HttpDelete("{dayOfWeek}")]
    [Authorize(Policy = PermissionKeys.ActionsScheduleEdit)]
    public async Task<OperationResult<bool>> Delete(int dayOfWeek, CancellationToken ct)
    {
        return await _manager.DeleteDayAsync(dayOfWeek, ct);
    }
}
