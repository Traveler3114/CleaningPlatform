using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly BookingManager _bookingManager;

    public BookingController(BookingManager bookingManager)
    {
        _bookingManager = bookingManager;
    }

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
}