using Microsoft.AspNetCore.Mvc;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Controllers;

[ApiController]
[Route("api/overrides")]
public class OverrideController : ControllerBase
{
    private readonly OverrideManager _manager;

    public OverrideController(OverrideManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<OperationResult<List<HourOverrideDto>>> Get()
    {
        var overrides = await _manager.GetOverridesAsync();
        return OperationResult<List<HourOverrideDto>>.Ok(overrides);
    }

    [HttpPost]
    public async Task<OperationResult<HourOverrideDto>> Post([FromBody] HourOverrideDto dto)
    {
        return await _manager.CreateOverrideAsync(dto);
    }

    [HttpDelete("{id}")]
    public async Task<OperationResult<bool>> Delete(int id)
    {
        return await _manager.DeleteOverrideAsync(id);
    }
}
