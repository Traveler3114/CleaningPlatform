using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/portal")]
[Authorize]
public class PortalDataController : ControllerBase
{
    private readonly PortalDataManager _portalManager;

    public PortalDataController(PortalDataManager portalManager) { _portalManager = portalManager; }

    private int? GetClientId()
    {
        var claim = User.FindFirst("client_id")?.Value;
        return claim is not null && int.TryParse(claim, out var id) ? id : null;
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<PortalDashboardResponse>>> GetDashboard(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Unauthorized(OperationResult<PortalDashboardResponse>.Fail("INVALID_TOKEN", "Invalid token."));
        var result = await _portalManager.GetDashboardAsync(clientId.Value, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("bookings")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<List<BookingResponse>>>> GetBookings(
        [FromQuery] string? status, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Unauthorized(OperationResult<List<BookingResponse>>.Fail("INVALID_TOKEN", "Invalid token."));
        var result = await _portalManager.GetBookingsAsync(clientId.Value, status, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("bookings/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<BookingResponse>>> GetBookingDetail(
        int id, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Unauthorized(OperationResult<BookingResponse>.Fail("INVALID_TOKEN", "Invalid token."));
        var result = await _portalManager.GetBookingDetailAsync(clientId.Value, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("invoices")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<List<InvoiceResponse>>>> GetInvoices(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Unauthorized(OperationResult<List<InvoiceResponse>>.Fail("INVALID_TOKEN", "Invalid token."));
        var result = await _portalManager.GetInvoicesAsync(clientId.Value, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("invoices/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<InvoiceResponse>>> GetInvoiceDetail(
        int id, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Unauthorized(OperationResult<InvoiceResponse>.Fail("INVALID_TOKEN", "Invalid token."));
        var result = await _portalManager.GetInvoiceDetailAsync(clientId.Value, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("profile")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<PortalProfileResponse>>> GetProfile(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Unauthorized(OperationResult<PortalProfileResponse>.Fail("INVALID_TOKEN", "Invalid token."));
        var result = await _portalManager.GetProfileAsync(clientId.Value, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
