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

    public DateOverrideController(DateOverrideManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<OperationResult<List<DateOverrideResponse>>> Get(CancellationToken ct)
    {
        return OperationResult<List<DateOverrideResponse>>.Ok(await _manager.GetOverridesAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ActionsOverrideManage)]
    public async Task<OperationResult<DateOverrideResponse>> Post([FromBody] DateOverrideRequest request, CancellationToken ct)
    {
        return await _manager.CreateOverrideAsync(request, ct);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ActionsOverrideManage)]
    public async Task<OperationResult<bool>> Delete(int id, CancellationToken ct)
    {
        return await _manager.DeleteOverrideAsync(id, ct);
    }
}
