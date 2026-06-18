using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/recurring")]
[Authorize]
public class RecurringScheduleController : ControllerBase
{
    private readonly RecurringScheduleManager _manager;
    public RecurringScheduleController(RecurringScheduleManager manager) { _manager = manager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<List<RecurringScheduleResponse>>> Get(
        [FromQuery] int? clientId, CancellationToken ct)
    {
        return Ok(await _manager.GetAllAsync(clientId, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<RecurringScheduleResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _manager.GetByIdAsync(id, ct));
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public async Task<ActionResult<RecurringScheduleResponse>> CreateFromBooking(
        int bookingId, [FromBody] CreateRecurringScheduleRequest request, CancellationToken ct)
    {
        return Ok(await _manager.CreateFromBookingAsync(bookingId, request, ct));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<RecurringScheduleResponse>> Update(
        int id, [FromBody] UpdateRecurringScheduleRequest request, CancellationToken ct)
    {
        return Ok(await _manager.UpdateAsync(id, request, ct));
    }

    [HttpPost("{id:int}/end")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<RecurringScheduleResponse>> EndSeries(
        int id, [FromBody] EndSeriesRequest request, CancellationToken ct)
    {
        return Ok(await _manager.EndSeriesAsync(id, request, ct));
    }

    [HttpPost("run-auto")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public async Task<ActionResult<List<GenerateResult>>> RunAutoGenerate(CancellationToken ct)
    {
        return Ok(await _manager.RunAutoGenerateAsync(ct));
    }
}
