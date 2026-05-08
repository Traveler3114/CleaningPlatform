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
    private readonly BookingManager _manager;

    public BookingController(BookingManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<OperationResult<List<BookingDto>>> Get([FromQuery] DateTime? date)
    {
        var bookings = date.HasValue
            ? await _manager.GetBookingsAsync(date.Value)
            : await _manager.GetAllBookingsAsync();
        return OperationResult<List<BookingDto>>.Ok(bookings);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<OperationResult<BookingDto>> Post([FromBody] CreateBookingDto dto)
    {
        return await _manager.CreateBookingAsync(dto);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "actions.booking.updateStatus")]
    public async Task<OperationResult<BookingDto>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        return await _manager.UpdateStatusAsync(id, request.Status);
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
