using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

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
    public async Task<ActionResult<List<DateOverrideResponse>>> Get(CancellationToken ct)
    {
        return Ok(await _manager.GetOverridesAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult<DateOverrideResponse>> Post([FromBody] DateOverrideRequest request, CancellationToken ct)
    {
        return Ok(await _manager.CreateOverrideAsync(request, ct));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ScheduleEdit)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _manager.DeleteOverrideAsync(id, ct);
        return NoContent();
    }
}