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
    public async Task<IActionResult> Get([FromQuery] DateTime? date)
    {
        if (date.HasValue)
        {
            var bookings = await _bookingManager.GetBookingsAsync(date.Value);
            return Ok(bookings);
        }
        var all = await _bookingManager.GetAllBookingsAsync();
        return Ok(all);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var detail = await _bookingManager.GetBookingDetailByIdAsync(id);
        if (detail is null)
            return NotFound("Booking not found.");
        return Ok(detail);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateBookingDto dto)
    {
        var result = await _bookingManager.CreateBookingAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
    {
        var result = await _bookingManager.UpdateStatusAsync(id, request.Status);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/assign")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<ActionResult<OperationResult<BookingDetailDto>>> AssignEmployee(int id, [FromBody] BookingAssignDto dto)
    {
        var result = await _bookingManager.AssignEmployeeAsync(id, dto.EmployeeId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:int}/services")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<ActionResult<OperationResult<BookingDetailDto>>> AddService(int id, [FromBody] AddBookingServiceDto dto)
    {
        var addResult = await _bookingManager.AddServiceAsync(id, dto.ServiceCatalogId, dto.EstimatedPrice, dto.Quantity);
        if (!addResult.Success)
            return BadRequest(addResult);

        if (dto.FinalPrice.HasValue && addResult.Data?.Services.LastOrDefault() is { } addedService)
        {
            var finalResult = await _bookingManager.UpdateServicePriceAsync(id, addedService.Id, dto.FinalPrice);
            if (!finalResult.Success)
                return BadRequest(finalResult);
            return Ok(finalResult);
        }

        return Ok(addResult);
    }

    [HttpDelete("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<ActionResult<OperationResult<string>>> RemoveService(int id, int serviceId)
    {
        var result = await _bookingManager.RemoveServiceAsync(id, serviceId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<ActionResult<OperationResult<BookingDetailDto>>> UpdateServicePrice(int id, int serviceId, [FromBody] UpdateBookingServicePriceDto dto)
    {
        var result = await _bookingManager.UpdateServicePriceAsync(id, serviceId, dto.FinalPrice);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
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
