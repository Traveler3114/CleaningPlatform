using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Models;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceManager _invoiceManager;
    private readonly IStringLocalizer<SharedResources> _localizer;
    public InvoiceController(InvoiceManager invoiceManager, IStringLocalizer<SharedResources> localizer) { _invoiceManager = invoiceManager; 
            _localizer = localizer;}

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InvoicesView)]
    public async Task<ActionResult<Paginated<InvoiceResponse>>> GetAll(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct)
    {
        return Ok(await _invoiceManager.GetAllAsync(pagination, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.InvoicesView)]
    public async Task<ActionResult<InvoiceResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _invoiceManager.GetByIdAsync(id, ct));
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.InvoicesCreate)]
    public async Task<ActionResult<InvoiceResponse>> CreateFromBooking(int bookingId, CancellationToken ct)
    {
        return Ok(await _invoiceManager.CreateFromBookingAsync(bookingId, User.GetEmployeeId(), ct));
    }

    [HttpPost("from-booking")]
    [Authorize(Policy = PermissionKeys.InvoicesCreate)]
    public async Task<ActionResult<InvoiceResponse>> CreateFromBookingPayload([FromBody] CreateInvoiceFromBookingRequest request, CancellationToken ct)
    {
        if (request.BookingId <= 0)
            throw new AppException("BOOKING_ID_REQUIRED", _localizer["err_booking_id_required"], 400);
        return Ok(await _invoiceManager.CreateFromBookingAsync(request.BookingId, User.GetEmployeeId(), ct));
    }

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = PermissionKeys.InvoicesEdit)]
    public async Task<ActionResult<InvoiceResponse>> RecordPayment(int id, [FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        return Ok(await _invoiceManager.RecordPaymentAsync(id, request, User.GetEmployeeId(), ct));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.InvoicesEdit)]
    public async Task<ActionResult<InvoiceResponse>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken ct)
    {
        return Ok(await _invoiceManager.UpdateStatusAsync(id, request.Status, ct));
    }
}