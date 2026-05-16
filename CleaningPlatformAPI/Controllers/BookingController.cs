using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
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
    public async Task<OperationResult<List<BookingResponse>>> Get([FromQuery] DateTime? date, CancellationToken ct)
    {
        if (date.HasValue)
            return OperationResult<List<BookingResponse>>.Ok(await _bookingManager.GetBookingsAsync(date.Value, ct));
        return OperationResult<List<BookingResponse>>.Ok(await _bookingManager.GetAllBookingsAsync(ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.BookingsView)]
    public async Task<OperationResult<BookingResponse>> GetById(int id, CancellationToken ct)
    {
        var detail = await _bookingManager.GetBookingDetailByIdAsync(id, ct);
        return detail is null
            ? OperationResult<BookingResponse>.Fail($"Booking #{id} was not found.")
            : OperationResult<BookingResponse>.Ok(detail);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<OperationResult<BookingResponse>> Post([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        return await _bookingManager.CreateBookingAsync(request, ct);
    }

    [HttpPost("admin")]
    [Authorize(Policy = PermissionKeys.BookingsCreate)]
    public Task<OperationResult<BookingResponse>> CreateAdmin([FromBody] CreateAdminBookingRequest request, CancellationToken ct)
    {
        return _bookingManager.CreateAdminBookingAsync(request, ct);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<OperationResult<BookingResponse>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        return await _bookingManager.UpdateStatusAsync(id, request.Status, ct);
    }

    [HttpPost("{id:int}/assignments")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<OperationResult<BookingResponse>> AddAssignment(int id, [FromBody] AssignEmployeeRequest request, CancellationToken ct)
    {
        return await _bookingManager.AddAssignmentAsync(id, request.EmployeeId, ct);
    }

    [HttpDelete("{id:int}/assignments/{assignmentId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<OperationResult<string>> RemoveAssignment(int id, int assignmentId, CancellationToken ct)
    {
        return await _bookingManager.RemoveAssignmentAsync(id, assignmentId, ct);
    }

    [HttpPost("{id:int}/services")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<OperationResult<BookingResponse>> AddService(int id, [FromBody] AddServiceRequest request, CancellationToken ct)
    {
        return await _bookingManager.AddServiceAsync(id, request.ServiceCatalogId, request.EstimatedPrice, request.Quantity, request.FinalPrice, request.Notes, ct);
    }

    [HttpDelete("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<OperationResult<string>> RemoveService(int id, int serviceId, CancellationToken ct)
    {
        return await _bookingManager.RemoveServiceAsync(id, serviceId, ct);
    }

    [HttpPut("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = PermissionKeys.BookingsEdit)]
    public async Task<OperationResult<BookingResponse>> UpdateServicePrice(int id, int serviceId, [FromBody] UpdateServicePriceRequest request, CancellationToken ct)
    {
        return await _bookingManager.UpdateServicePriceAsync(id, serviceId, request.FinalPrice, ct);
    }
}
