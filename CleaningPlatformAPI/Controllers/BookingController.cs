// CleaningPlatformAPI/Controllers/BookingController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}

public class StatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}