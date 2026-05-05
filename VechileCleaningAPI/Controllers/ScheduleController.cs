using Microsoft.AspNetCore.Mvc;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Controllers;

[ApiController]
[Route("api/schedule")]
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
    public async Task<OperationResult<WeeklyScheduleDto>> Post([FromBody] WeeklyScheduleDto dto)
    {
        return await _manager.CreateDayAsync(dto);
    }

    [HttpPut("{dayOfWeek}")]
    public async Task<OperationResult<WeeklyScheduleDto>> Put(int dayOfWeek, [FromBody] WeeklyScheduleDto dto)
    {
        return await _manager.UpdateDayAsync(dayOfWeek, dto);
    }

    [HttpDelete("{dayOfWeek}")]
    public async Task<OperationResult<bool>> Delete(int dayOfWeek)
    {
        return await _manager.DeleteDayAsync(dayOfWeek);
    }
}
