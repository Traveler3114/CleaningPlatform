using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Models;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly BookingManager _bookingManager;
    private readonly SopManager _sopManager;

    public BookingController(BookingManager bookingManager, SopManager sopManager) { _bookingManager = bookingManager; _sopManager = sopManager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<object>> Get(
        [FromQuery] DateTime? date,
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        if (date.HasValue)
            return Ok(await _bookingManager.GetBookingsAsync(date.Value, ct));

        return Ok(await _bookingManager.GetAllBookingsAsync(pagination, status, ct));
    }

    [HttpGet("{id:int}", Name = "GetBookingById")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _bookingManager.GetBookingDetailByIdAsync(id, ct));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Post(
        [FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        return Ok(await _bookingManager.CreateBookingAsync(request, ct));
    }

    [HttpPost("admin")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public async Task<ActionResult<BookingResponse>> CreateAdmin(
        [FromBody] CreateAdminBookingRequest request, CancellationToken ct)
    {
        return Ok(await _bookingManager.CreateAdminBookingAsync(request, ct));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingResponse>> UpdateStatus(
        int id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        return Ok(await _bookingManager.UpdateStatusAsync(id, request.Status, ct));
    }

    [HttpPost("{id:int}/assignments")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingResponse>> AddAssignment(
        int id, [FromBody] AssignEmployeeRequest request, CancellationToken ct)
    {
        return Ok(await _bookingManager.AddAssignmentAsync(id, request.EmployeeId, ct));
    }

    [HttpDelete("{id:int}/assignments/{employeeId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult> RemoveAssignment(
        int id, int employeeId, CancellationToken ct)
    {
        await _bookingManager.RemoveAssignmentAsync(id, employeeId, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/services")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingResponse>> AddService(
        int id, [FromBody] AddServiceRequest request, CancellationToken ct)
    {
        return Ok(await _bookingManager.AddServiceAsync(
            id, request.ServiceCatalogId, request.EstimatedPrice,
            request.Quantity, request.FinalPrice, request.Notes, ct));
    }

    [HttpDelete("{id:int}/services/{serviceCatalogId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult> RemoveService(
        int id, int serviceCatalogId, CancellationToken ct)
    {
        await _bookingManager.RemoveServiceAsync(id, serviceCatalogId, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/services/{serviceCatalogId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingResponse>> UpdateServicePrice(
        int id, int serviceCatalogId, [FromBody] UpdateServicePriceRequest request, CancellationToken ct)
    {
        return Ok(await _bookingManager.UpdateServicePriceAsync(id, serviceCatalogId, request.FinalPrice, ct));
    }

    [HttpGet("{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<List<BookingSopAssignmentResponse>>> GetBookingSops(int id, CancellationToken ct)
    {
        return Ok(await _sopManager.GetBookingSopsAsync(id, ct));
    }

    [HttpPost("{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<BookingSopAssignmentResponse>> AssignSop(int id, [FromBody] AssignSopRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.AssignSopToBookingAsync(id, request, ct));
    }

    [HttpGet("{id:int}/sops/{sopTemplateId:int}/checklist")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<List<ChecklistResponseResponse>>> GetSopChecklist(
        int id, int sopTemplateId, CancellationToken ct)
    {
        return Ok(await _sopManager.GetChecklistForSopAssignmentAsync(id, sopTemplateId, ct));
    }

    [HttpPut("{id:int}/sops/{sopTemplateId:int}/checklist/{checklistItemId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<ChecklistResponseResponse>> CompleteChecklistItem(
        int id, int sopTemplateId, int checklistItemId, [FromBody] CompleteChecklistItemRequest request, CancellationToken ct)
    {
        return Ok(await _sopManager.CompleteChecklistItemAsync(id, sopTemplateId, checklistItemId, request, ct));
    }

    [HttpGet("employee/assigned")]
    [Authorize]
    public async Task<ActionResult<List<BookingResponse>>> GetAssignedForEmployee(
        [FromQuery] int? employeeId,
        [FromQuery] DateTime? date,
        CancellationToken ct)
    {
        var targetId = employeeId ?? User.GetEmployeeId();
        if (targetId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");

        var currentUserId = User.GetEmployeeId();
        if (currentUserId != targetId && !User.IsInRole(RoleNames.Owner) && !User.IsInRole(RoleNames.Admin))
            return Forbid();

        if (date.HasValue)
            return Ok(await _bookingManager.GetAssignedBookingsForEmployeeByDateAsync(targetId.Value, date.Value, ct));
        else
            return Ok(await _bookingManager.GetAssignedBookingsForEmployeeAsync(targetId.Value, ct));
    }
}