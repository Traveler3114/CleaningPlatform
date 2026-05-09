using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<InvoiceSummaryDto>>> GetAll()
    {
        var invoices = await _invoiceManager.GetAllAsync();
        return OperationResult<List<InvoiceSummaryDto>>.Ok(invoices);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.PagesBookings)]
    public async Task<OperationResult<InvoiceDetailDto>> GetById(int id)
    {
        var invoice = await _invoiceManager.GetByIdAsync(id);
        return invoice is null
            ? OperationResult<InvoiceDetailDto>.Fail("Invoice not found.")
            : OperationResult<InvoiceDetailDto>.Ok(invoice);
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailDto>> CreateFromBooking(int bookingId)
    {
        var employeeId = ParseCurrentEmployeeId();
        return await _invoiceManager.CreateFromBookingAsync(bookingId, employeeId);
    }

    [HttpPost("from-booking")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailDto>> CreateFromBookingPayload([FromBody] CreateInvoiceFromBookingDto dto)
    {
        if (dto.BookingId <= 0)
            return OperationResult<InvoiceDetailDto>.Fail("Booking id is required.");
        return await _invoiceManager.CreateFromBookingAsync(dto.BookingId, ParseCurrentEmployeeId());
    }

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailDto>> RecordPayment(int id, [FromBody] RecordPaymentDto dto)
    {
        return await _invoiceManager.RecordPaymentAsync(id, dto, ParseCurrentEmployeeId());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<OperationResult<InvoiceDetailDto>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusDto dto)
    {
        return await _invoiceManager.UpdateStatusAsync(id, dto.Status);
    }

    private int? ParseCurrentEmployeeId()
    {
        var raw = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(raw, out var employeeId) ? employeeId : null;
    }
}