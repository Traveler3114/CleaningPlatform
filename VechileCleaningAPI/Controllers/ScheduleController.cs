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

    [HttpPut("{dayOfWeek}")]
    public async Task<OperationResult<WeeklyScheduleDto>> Put(int dayOfWeek, [FromBody] WeeklyScheduleDto dto)
    {
        return await _manager.UpdateDayAsync(dayOfWeek, dto);
    }
}
