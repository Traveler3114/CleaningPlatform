using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/availability")]
[AllowAnonymous]
public class AvailabilityController : ControllerBase
{
    private readonly AvailabilityManager _manager;

    public AvailabilityController(AvailabilityManager manager) { _manager = manager; }

    [HttpGet]
    public async Task<ActionResult<List<AvailabilityResponse>>> Get([FromQuery] DateTime date, CancellationToken ct)
    {
        return Ok(await _manager.GetSlotsAsync(date, ct));
    }
}