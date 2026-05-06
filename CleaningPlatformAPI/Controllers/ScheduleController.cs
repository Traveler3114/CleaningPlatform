using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<WeeklyScheduleDto>>> Get()
    {
        var schedule = await _manager.GetScheduleAsync();
        return OperationResult<List<WeeklyScheduleDto>>.Ok(schedule);
    }

    [HttpPost]
    [Authorize(Policy = "actions.schedule.edit")]
    public async Task<OperationResult<WeeklyScheduleDto>> Post([FromBody] WeeklyScheduleDto dto)
    {
        return await _manager.CreateDayAsync(dto);
    }

    [HttpPut("{dayOfWeek}")]
    [Authorize(Policy = "actions.schedule.edit")]
    public async Task<OperationResult<WeeklyScheduleDto>> Put(int dayOfWeek, [FromBody] WeeklyScheduleDto dto)
    {
        return await _manager.UpdateDayAsync(dayOfWeek, dto);
    }

    [HttpDelete("{dayOfWeek}")]
    [Authorize(Policy = "actions.schedule.edit")]
    public async Task<OperationResult<bool>> Delete(int dayOfWeek)
    {
        return await _manager.DeleteDayAsync(dayOfWeek);
    }
}
