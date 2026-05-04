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
    public async Task<OperationResult<List<SlotOverrideDto>>> Get()
    {
        var overrides = await _manager.GetOverridesAsync();
        return OperationResult<List<SlotOverrideDto>>.Ok(overrides);
    }

    [HttpPost]
    public async Task<OperationResult<SlotOverrideDto>> Post([FromBody] SlotOverrideDto dto)
    {
        return await _manager.CreateOverrideAsync(dto);
    }

    [HttpDelete("{id}")]
    public async Task<OperationResult<bool>> Delete(int id)
    {
        return await _manager.DeleteOverrideAsync(id);
    }
}
