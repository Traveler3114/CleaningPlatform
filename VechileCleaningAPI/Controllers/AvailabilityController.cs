using Microsoft.AspNetCore.Mvc;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly AvailabilityManager _manager;

    public AvailabilityController(AvailabilityManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<OperationResult<List<AvailabilityDto>>> Get([FromQuery] DateTime date)
    {
        var slots = await _manager.GetSlotsAsync(date);
        return OperationResult<List<AvailabilityDto>>.Ok(slots);
    }
}
