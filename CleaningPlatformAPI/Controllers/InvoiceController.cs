using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceManager _invoiceManager;
    public InvoiceController(InvoiceManager invoiceManager) { _invoiceManager = invoiceManager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InvoicesView)]
    public async Task<ActionResult<OperationResult<PagedResult<InvoiceResponse>>>> GetAll(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct)
    {
        var paged = await _invoiceManager.GetAllAsync(pagination, ct);
        return Ok(OperationResult<PagedResult<InvoiceResponse>>.Ok(paged));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.InvoicesView)]
    public async Task<ActionResult<OperationResult<InvoiceResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _invoiceManager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.InvoicesCreate)]
    public async Task<ActionResult<OperationResult<InvoiceResponse>>> CreateFromBooking(int bookingId, CancellationToken ct)
    {
        var result = await _invoiceManager.CreateFromBookingAsync(bookingId, User.GetEmployeeId(), ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("from-booking")]
    [Authorize(Policy = PermissionKeys.InvoicesCreate)]
    public async Task<ActionResult<OperationResult<InvoiceResponse>>> CreateFromBookingPayload([FromBody] CreateInvoiceFromBookingRequest request, CancellationToken ct)
    {
        if (request.BookingId <= 0)
            return BadRequest(OperationResult<InvoiceResponse>.Fail("Booking ID is required."));
        var result = await _invoiceManager.CreateFromBookingAsync(request.BookingId, User.GetEmployeeId(), ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = PermissionKeys.InvoicesEdit)]
    public async Task<ActionResult<OperationResult<InvoiceResponse>>> RecordPayment(int id, [FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        var result = await _invoiceManager.RecordPaymentAsync(id, request, User.GetEmployeeId(), ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.InvoicesEdit)]
    public async Task<ActionResult<OperationResult<InvoiceResponse>>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken ct)
    {
        var result = await _invoiceManager.UpdateStatusAsync(id, request.Status, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}