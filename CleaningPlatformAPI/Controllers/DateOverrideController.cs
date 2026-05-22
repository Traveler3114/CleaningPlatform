using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/overrides")]
[Authorize]
public class DateOverrideController : ControllerBase
{
    private readonly DateOverrideManager _manager;
    public DateOverrideController(DateOverrideManager manager) { _manager = manager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.ScheduleView)]
    public async Task<ActionResult<OperationResult<List<DateOverrideResponse>>>> Get(CancellationToken ct)
    {
        var result = await _manager.GetOverridesAsync(ct);
        return Ok(OperationResult<List<DateOverrideResponse>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<OperationResult<DateOverrideResponse>>> Post([FromBody] DateOverrideRequest request, CancellationToken ct)
    {
        var result = await _manager.CreateOverrideAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<OperationResult<bool>>> Delete(int id, CancellationToken ct)
    {
        var result = await _manager.DeleteOverrideAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}