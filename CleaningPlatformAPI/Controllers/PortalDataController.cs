using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[Route("api/portal")]
[Authorize]
public class PortalDataController : ControllerBase
{
    private readonly PortalDataManager _portalManager;

    public PortalDataController(PortalDataManager portalManager)
    {
        _portalManager = portalManager;
    }

    private int GetClientId()
    {
        var claim = User.FindFirst("client_id")?.Value;
        return claim != null ? int.Parse(claim) : 0;
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<PortalDashboardResponse>>> GetDashboard(CancellationToken ct)
    {
        var result = await _portalManager.GetDashboardAsync(GetClientId(), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("bookings")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<List<PortalBookingSummary>>>> GetBookings(
        [FromQuery] string? status, CancellationToken ct)
    {
        var result = await _portalManager.GetBookingsAsync(GetClientId(), status, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("bookings/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<PortalBookingDetailResponse>>> GetBookingDetail(
        int id, CancellationToken ct)
    {
        var result = await _portalManager.GetBookingDetailAsync(GetClientId(), id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("invoices")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<List<PortalInvoiceSummary>>>> GetInvoices(CancellationToken ct)
    {
        var result = await _portalManager.GetInvoicesAsync(GetClientId(), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("invoices/{id:int}")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<PortalInvoiceDetailResponse>>> GetInvoiceDetail(
        int id, CancellationToken ct)
    {
        var result = await _portalManager.GetInvoiceDetailAsync(GetClientId(), id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("profile")]
    [Authorize(Policy = "PortalOnly")]
    public async Task<ActionResult<OperationResult<PortalProfileResponse>>> GetProfile(CancellationToken ct)
    {
        var result = await _portalManager.GetProfileAsync(GetClientId(), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
