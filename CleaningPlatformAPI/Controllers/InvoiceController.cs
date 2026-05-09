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
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _invoiceManager.GetAllAsync();
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.PagesBookings)]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _invoiceManager.GetByIdAsync(id);
        if (invoice == null)
            return NotFound("Invoice not found.");
        return Ok(invoice);
    }

    [HttpPost("from-booking/{bookingId:int}")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<IActionResult> CreateFromBooking(int bookingId)
    {
        var employeeId = ParseCurrentEmployeeId();
        var result = await _invoiceManager.CreateFromBookingAsync(bookingId, employeeId);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPost("from-booking")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<IActionResult> CreateFromBookingPayload([FromBody] CreateInvoiceFromBookingDto dto)
    {
        if (dto.BookingId <= 0)
            return BadRequest("Booking id is required.");

        var employeeId = ParseCurrentEmployeeId();
        var result = await _invoiceManager.CreateFromBookingAsync(dto.BookingId, employeeId);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<IActionResult> RecordPayment(int id, [FromBody] RecordPaymentDto dto)
    {
        var employeeId = ParseCurrentEmployeeId();
        var result = await _invoiceManager.RecordPaymentAsync(id, dto, employeeId);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.ActionsBookingUpdateStatus)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusDto dto)
    {
        var result = await _invoiceManager.UpdateStatusAsync(id, dto.Status);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    private int? ParseCurrentEmployeeId()
    {
        var raw = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(raw, out var employeeId) ? employeeId : null;
    }
}
