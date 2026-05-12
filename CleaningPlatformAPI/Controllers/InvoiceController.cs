using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceManager _invoiceManager;

    public InvoiceController(InvoiceManager invoiceManager)
    {
        _invoiceManager = invoiceManager;
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.PagesBookings)]
    public async Task<OperationResult<List<InvoiceSummaryResponse>>> GetAll(CancellationToken ct)
    {
        return OperationResult<List<InvoiceSummaryResponse>>.Ok(await _invoiceManager.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.PagesBookings)]
    public async Task<OperationResult<InvoiceDetailResponse>> GetById(int id, CancellationToken ct)
    {
        var invoice = await _invoiceManager.GetByIdAsync(id, ct);
        return invoice is null
            ? OperationResult<InvoiceDetailResponse>.Fail("Invoice not found.")
            : OperationResult<InvoiceDetailResponse>.Ok(invoice);
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailResponse>> CreateFromBooking(int bookingId, CancellationToken ct)
    {
        return await _invoiceManager.CreateFromBookingAsync(bookingId, ParseCurrentEmployeeId(), ct);
    }

    [HttpPost("from-booking")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailResponse>> CreateFromBookingPayload([FromBody] CreateInvoiceFromBookingRequest request, CancellationToken ct)
    {
        if (request.BookingId <= 0)
            return OperationResult<InvoiceDetailResponse>.Fail("Booking id is required.");

        return await _invoiceManager.CreateFromBookingAsync(request.BookingId, ParseCurrentEmployeeId(), ct);
    }

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailResponse>> RecordPayment(int id, [FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        return await _invoiceManager.RecordPaymentAsync(id, request, ParseCurrentEmployeeId(), ct);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailResponse>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken ct)
    {
        return await _invoiceManager.UpdateStatusAsync(id, request.Status, ct);
    }

    private int? ParseCurrentEmployeeId()
    {
        var raw = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(raw, out var employeeId) ? employeeId : null;
    }
}
