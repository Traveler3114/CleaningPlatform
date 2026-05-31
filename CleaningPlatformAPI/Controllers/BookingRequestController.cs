using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/booking-requests")]
public class BookingRequestController : ControllerBase
{
    private readonly BookingRequestManager _manager;

    public BookingRequestController(BookingRequestManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<PagedResult<BookingRequestResponse>>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _manager.GetAllAsync(pagination, status, ct);
        return Ok(OperationResult<PagedResult<BookingRequestResponse>>.Ok(result));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<BookingRequestResponse>>> GetById(int id, CancellationToken ct = default)
    {
        var result = await _manager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResult<BookingRequestResponse>>> Create(
        [FromBody] CreateBookingRequestRequest dto,
        CancellationToken ct = default)
    {
        var result = await _manager.CreateAsync(dto, ct);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : UnprocessableEntity(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingRequestResponse>>> Update(
        int id,
        [FromBody] UpdateBookingRequestRequest dto,
        CancellationToken ct = default)
    {
        var result = await _manager.UpdateAsync(id, dto, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id}/send")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingRequestResponse>>> SendToCustomer(
        int id,
        CancellationToken ct = default)
    {
        var result = await _manager.SendToCustomerAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id}/confirm")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> AdminConfirm(
        int id,
        CancellationToken ct = default)
    {
        var result = await _manager.AdminConfirmAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("customer-preview")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResult<CustomerPreviewResponse>>> CustomerPreview(
        [FromQuery] string token,
        CancellationToken ct = default)
    {
        var result = await _manager.CustomerPreviewAsync(token, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("customer-confirm")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResult<string>>> CustomerConfirm(
        [FromBody] CustomerConfirmRequest dto,
        CancellationToken ct = default)
    {
        var result = await _manager.CustomerConfirmAsync(dto.Token, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("customer-cancel")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResult<string>>> CustomerCancel(
        [FromBody] CustomerConfirmRequest dto,
        CancellationToken ct = default)
    {
        var result = await _manager.CustomerCancelAsync(dto.Token, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}
