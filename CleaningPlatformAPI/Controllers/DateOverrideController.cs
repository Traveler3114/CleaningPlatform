using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<DateOverrideDto>>> Get()
    {
        var overrides = await _manager.GetOverridesAsync();
        return OperationResult<List<DateOverrideDto>>.Ok(overrides);
    }

    [HttpPost]
    [Authorize(Policy = "actions.override.manage")]
    public async Task<OperationResult<DateOverrideDto>> Post([FromBody] DateOverrideDto dto)
    {
        return await _manager.CreateOverrideAsync(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "actions.override.manage")]
    public async Task<OperationResult<bool>> Delete(int id)
    {
        return await _manager.DeleteOverrideAsync(id);
    }
}
