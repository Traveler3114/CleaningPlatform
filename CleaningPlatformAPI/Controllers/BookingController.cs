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

    [HttpDelete("{id:int}/assignments/{assignmentId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<string>>> RemoveAssignment(
        int id, int assignmentId, CancellationToken ct)
    {
        var result = await _bookingManager.RemoveAssignmentAsync(id, assignmentId, ct);
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

    [HttpDelete("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<string>>> RemoveService(
        int id, int serviceId, CancellationToken ct)
    {
        var result = await _bookingManager.RemoveServiceAsync(id, serviceId, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<ActionResult<OperationResult<BookingResponse>>> UpdateServicePrice(
        int id, int serviceId, [FromBody] UpdateServicePriceRequest request, CancellationToken ct)
    {
        var result = await _bookingManager.UpdateServicePriceAsync(id, serviceId, request.FinalPrice, ct);
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

    [HttpGet("employee/assigned")]
    [Authorize] // or [Authorize(Policy = PermissionKeys.BookingsView)] – any authenticated user can see their own assigned bookings
    public async Task<ActionResult<OperationResult<List<BookingResponse>>>> GetAssignedForEmployee(
        [FromQuery] int? employeeId,
        CancellationToken ct)
    {
        var targetId = employeeId ?? User.GetEmployeeId();
        if (targetId == null)
            return Unauthorized(OperationResult<List<BookingResponse>>.Fail("Invalid token."));

        // Optional: Check that the requesting user is allowed to see this employee's assignments
        var currentUserId = User.GetEmployeeId();
        if (currentUserId != targetId && !User.IsInRole(RoleNames.Owner) && !User.IsInRole(RoleNames.Admin))
            return Forbid();

        var result = await _bookingManager.GetAssignedBookingsForEmployeeAsync(targetId.Value, ct);
        return Ok(OperationResult<List<BookingResponse>>.Ok(result));
    }
}