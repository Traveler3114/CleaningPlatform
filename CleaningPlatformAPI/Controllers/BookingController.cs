using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<OperationResult<object>>> Get(
        [FromQuery] DateTime? date,
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        if (date.HasValue)
        {
            var daily = await _bookingManager.GetBookingsAsync(date.Value, ct);
            return Ok(OperationResult<List<BookingResponse>>.Ok(daily));
        }

        var paged = await _bookingManager.GetAllBookingsAsync(pagination, status, ct);
        return Ok(OperationResult<PagedResult<BookingResponse>>.Ok(paged));
    }

    [HttpGet("{id:int}", Name = "GetBookingById")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _bookingManager.GetBookingDetailByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<OperationResult<BookingResponse>>> Post(
        [FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.CreateBookingAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("admin")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> CreateAdmin(
        [FromBody] CreateAdminBookingRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.CreateAdminBookingAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> UpdateStatus(
        int id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.UpdateStatusAsync(id, request.Status, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id:int}/assignments")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> AddAssignment(
        int id, [FromBody] AssignEmployeeRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.AddAssignmentAsync(id, request.EmployeeId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id:int}/assignments/{employeeId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<string>>> RemoveAssignment(
        int id, int employeeId, CancellationToken ct)
    {
        var result = await _bookingManager.RemoveAssignmentAsync(id, employeeId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("{id:int}/services")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> AddService(
        int id, [FromBody] AddServiceRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.AddServiceAsync(
            id, request.ServiceCatalogId, request.EstimatedPrice,
            request.Quantity, request.FinalPrice, request.Notes, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id:int}/services/{serviceCatalogId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<string>>> RemoveService(
        int id, int serviceCatalogId, CancellationToken ct)
    {
        var result = await _bookingManager.RemoveServiceAsync(id, serviceCatalogId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}/services/{serviceCatalogId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> UpdateServicePrice(
        int id, int serviceCatalogId, [FromBody] UpdateServicePriceRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.UpdateServicePriceAsync(id, serviceCatalogId, request.FinalPrice, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<List<BookingSopAssignmentResponse>>>> GetBookingSops(int id, CancellationToken ct)
    {
        var assignments = await _sopManager.GetBookingSopsAsync(id, ct);
        return Ok(OperationResult<List<BookingSopAssignmentResponse>>.Ok(assignments));
    }

    [HttpPost("{id:int}/sops")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingSopAssignmentResponse>>> AssignSop(int id, [FromBody] AssignSopRequest request, CancellationToken ct)
    {
        var result = await _sopManager.AssignSopToBookingAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("{id:int}/sops/{sopTemplateId:int}/checklist")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<ActionResult<OperationResult<List<ChecklistResponseResponse>>>> GetSopChecklist(
        int id, int sopTemplateId, CancellationToken ct)
    {
        var items = await _sopManager.GetChecklistForSopAssignmentAsync(id, sopTemplateId, ct);
        return Ok(OperationResult<List<ChecklistResponseResponse>>.Ok(items));
    }

    [HttpPut("{id:int}/sops/{sopTemplateId:int}/checklist/{checklistItemId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<ChecklistResponseResponse>>> CompleteChecklistItem(
        int id, int sopTemplateId, int checklistItemId, [FromBody] CompleteChecklistItemRequest request, CancellationToken ct)
    {
        var result = await _sopManager.CompleteChecklistItemAsync(id, sopTemplateId, checklistItemId, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpGet("employee/assigned")]
    [Authorize]
    public async Task<ActionResult<OperationResult<List<BookingResponse>>>> GetAssignedForEmployee(
        [FromQuery] int? employeeId,
        [FromQuery] DateTime? date,
        CancellationToken ct)
    {
        var targetId = employeeId ?? User.GetEmployeeId();
        if (targetId is null)
            return Unauthorized(OperationResult<List<BookingResponse>>.Fail("Invalid token."));

        var currentUserId = User.GetEmployeeId();
        if (currentUserId != targetId && !User.IsInRole(RoleNames.Owner) && !User.IsInRole(RoleNames.Admin))
            return Forbid();

        List<BookingResponse> result;
        if (date.HasValue)
            result = await _bookingManager.GetAssignedBookingsForEmployeeByDateAsync(targetId.Value, date.Value, ct);
        else
            result = await _bookingManager.GetAssignedBookingsForEmployeeAsync(targetId.Value, ct);

        return Ok(OperationResult<List<BookingResponse>>.Ok(result));
    }
}