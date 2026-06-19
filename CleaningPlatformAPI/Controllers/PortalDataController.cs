using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    private readonly IStringLocalizer<SharedResources> _localizer;

    public PortalDataController(PortalDataManager portalManager, IStringLocalizer<SharedResources> localizer) { _portalManager = portalManager; _localizer = localizer; }

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
            throw new AppException("INVALID_TOKEN", _localizer["error_invalid_token"], 401);
        return Ok(await _portalManager.GetDashboardAsync(clientId.Value, ct));
    }

    [HttpGet("bookings")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<List<BookingResponse>>> GetBookings(
        [FromQuery] string? status, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            throw new AppException("INVALID_TOKEN", _localizer["error_invalid_token"], 401);
        return Ok(await _portalManager.GetBookingsAsync(clientId.Value, status, ct));
    }

    [HttpGet("bookings/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<BookingResponse>> GetBookingDetail(
        int id, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            throw new AppException("INVALID_TOKEN", _localizer["error_invalid_token"], 401);
        return Ok(await _portalManager.GetBookingDetailAsync(clientId.Value, id, ct));
    }

    [HttpGet("invoices")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<List<InvoiceResponse>>> GetInvoices(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            throw new AppException("INVALID_TOKEN", _localizer["error_invalid_token"], 401);
        return Ok(await _portalManager.GetInvoicesAsync(clientId.Value, ct));
    }

    [HttpGet("invoices/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<InvoiceResponse>> GetInvoiceDetail(
        int id, CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            throw new AppException("INVALID_TOKEN", _localizer["error_invalid_token"], 401);
        return Ok(await _portalManager.GetInvoiceDetailAsync(clientId.Value, id, ct));
    }

    [HttpGet("profile")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<PortalProfileResponse>> GetProfile(CancellationToken ct)
    {
        var clientId = GetClientId();
        if (clientId is null)
            throw new AppException("INVALID_TOKEN", _localizer["error_invalid_token"], 401);
        return Ok(await _portalManager.GetProfileAsync(clientId.Value, ct));
    }
}
