using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Models;

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
    public async Task<ActionResult<Paginated<BookingRequestResponse>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        return Ok(await _manager.GetAllAsync(pagination, status, ct));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<BookingRequestResponse>> GetById(int id, CancellationToken ct = default)
    {
        return Ok(await _manager.GetByIdAsync(id, ct));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<BookingRequestResponse>> Create(
        [FromBody] CreateBookingRequestRequest dto,
        CancellationToken ct = default)
    {
        var created = await _manager.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingRequestResponse>> Update(
        int id,
        [FromBody] UpdateBookingRequestRequest dto,
        CancellationToken ct = default)
    {
        return Ok(await _manager.UpdateAsync(id, dto, ct));
    }

    [HttpPost("{id}/send")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingRequestResponse>> SendToCustomer(
        int id,
        CancellationToken ct = default)
    {
        return Ok(await _manager.SendToCustomerAsync(id, ct));
    }

    [HttpPost("{id}/confirm")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingResponse>> AdminConfirm(
        int id,
        CancellationToken ct = default)
    {
        return Ok(await _manager.AdminConfirmAsync(id, ct));
    }

    [HttpGet("customer-preview")]
    [AllowAnonymous]
    public async Task<ActionResult<CustomerPreviewResponse>> CustomerPreview(
        [FromQuery] string token,
        CancellationToken ct = default)
    {
        return Ok(await _manager.CustomerPreviewAsync(token, ct));
    }

    [HttpPost("customer-confirm")]
    [AllowAnonymous]
    public async Task<ActionResult> CustomerConfirm(
        [FromBody] CustomerConfirmRequest dto,
        CancellationToken ct = default)
    {
        await _manager.CustomerConfirmAsync(dto.Token, ct);
        return NoContent();
    }

    [HttpPost("customer-cancel")]
    [AllowAnonymous]
    public async Task<ActionResult> CustomerCancel(
        [FromBody] CustomerConfirmRequest dto,
        CancellationToken ct = default)
    {
        await _manager.CustomerCancelAsync(dto.Token, ct);
        return NoContent();
    }
}
