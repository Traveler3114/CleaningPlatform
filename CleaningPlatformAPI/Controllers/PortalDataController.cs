using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<PortalDashboardResponse>> GetDashboard(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");
        return Ok(await _portalManager.GetDashboardAsync(clientId.Value, ct));
    }

    [HttpGet("bookings")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<List<BookingResponse>>> GetBookings(
        [FromQuery] string? status, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");
        return Ok(await _portalManager.GetBookingsAsync(clientId.Value, status, ct));
    }

    [HttpGet("bookings/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<BookingResponse>> GetBookingDetail(
        int id, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");
        return Ok(await _portalManager.GetBookingDetailAsync(clientId.Value, id, ct));
    }

    [HttpGet("invoices")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<List<InvoiceResponse>>> GetInvoices(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");
        return Ok(await _portalManager.GetInvoicesAsync(clientId.Value, ct));
    }

    [HttpGet("invoices/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<InvoiceResponse>> GetInvoiceDetail(
        int id, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");
        return Ok(await _portalManager.GetInvoiceDetailAsync(clientId.Value, id, ct));
    }

    [HttpGet("profile")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<PortalProfileResponse>> GetProfile(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");
        return Ok(await _portalManager.GetProfileAsync(clientId.Value, ct));
    }
}
