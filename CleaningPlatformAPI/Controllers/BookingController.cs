// CleaningPlatformAPI/Controllers/BookingController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<BookingDto>>> Get([FromQuery] DateTime? date)
    {
        if (date.HasValue)
        {
            var bookings = await _bookingManager.GetBookingsAsync(date.Value);
            return OperationResult<List<BookingDto>>.Ok(bookings);
        }
        var all = await _bookingManager.GetAllBookingsAsync();
        return OperationResult<List<BookingDto>>.Ok(all);
    }

    [HttpGet("{id:int}")]
    public async Task<OperationResult<BookingDetailDto>> GetById(int id)
    {
        var detail = await _bookingManager.GetBookingDetailByIdAsync(id);
        return detail is null
            ? OperationResult<BookingDetailDto>.Fail("Booking not found.")
            : OperationResult<BookingDetailDto>.Ok(detail);
    }

    [HttpPost]
    public async Task<OperationResult<BookingDto>> Post([FromBody] CreateBookingDto dto)
    {
        return await _bookingManager.CreateBookingAsync(dto);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<OperationResult<BookingDto>> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
    {
        return await _bookingManager.UpdateStatusAsync(id, request.Status);
    }

    [HttpPut("{id:int}/assign")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<OperationResult<BookingDetailDto>> AssignEmployee(int id, [FromBody] BookingAssignDto dto)
    {
        return await _bookingManager.AssignEmployeeAsync(id, dto.EmployeeId);
    }

    [HttpPost("{id:int}/services")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<OperationResult<BookingDetailDto>> AddService(int id, [FromBody] AddBookingServiceDto dto)
    {
        return await _bookingManager.AddServiceAsync(
            id,
            dto.ServiceCatalogId,
            dto.EstimatedPrice,
            dto.Quantity,
            dto.FinalPrice,
            dto.Notes);
    }

    [HttpDelete("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<OperationResult<string>> RemoveService(int id, int serviceId)
    {
        return await _bookingManager.RemoveServiceAsync(id, serviceId);
    }

    [HttpPut("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<OperationResult<BookingDetailDto>> UpdateServicePrice(int id, int serviceId, [FromBody] UpdateBookingServicePriceDto dto)
    {
        return await _bookingManager.UpdateServicePriceAsync(id, serviceId, dto.FinalPrice);
    }
}

public class StatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateBookingServicePriceDto
{
    public decimal? FinalPrice { get; set; }
}