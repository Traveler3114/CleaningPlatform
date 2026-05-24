using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

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
    public async Task<ActionResult<OperationResult<List<RecurringScheduleResponse>>>> Get(
        [FromQuery] int? clientId, CancellationToken ct)
    {
        var result = await _manager.GetAllAsync(clientId, ct);
        return Ok(OperationResult<List<RecurringScheduleResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<RecurringScheduleResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _manager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public async Task<ActionResult<OperationResult<RecurringScheduleResponse>>> CreateFromBooking(
        int bookingId, [FromBody] CreateRecurringScheduleRequest request, CancellationToken ct)
    {
        var result = await _manager.CreateFromBookingAsync(bookingId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<RecurringScheduleResponse>>> Update(
        int id, [FromBody] UpdateRecurringScheduleRequest request, CancellationToken ct)
    {
        var result = await _manager.UpdateAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id:int}/end")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<RecurringScheduleResponse>>> EndSeries(
        int id, [FromBody] EndSeriesRequest request, CancellationToken ct)
    {
        var result = await _manager.EndSeriesAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("run-auto")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public async Task<ActionResult<OperationResult<List<GenerateResult>>>> RunAutoGenerate(CancellationToken ct)
    {
        var result = await _manager.RunAutoGenerateAsync(ct);
        return Ok(OperationResult<List<GenerateResult>>.Ok(result));
    }
}
