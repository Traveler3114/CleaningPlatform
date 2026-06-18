using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeManager _userManager;

    public EmployeeController(EmployeeManager userManager) { _userManager = userManager; }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
    {
        var userId = User.GetEmployeeId();
        if (userId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");

        return Ok(await _userManager.GetByIdAsync(userId.Value, ct));
    }

    [HttpGet("available-for-booking/{bookingId:int}")]
    public async Task<ActionResult<List<AvailableEmployeeResponse>>> GetAvailableForBooking(int bookingId, CancellationToken ct)
    {
        return Ok(await _userManager.GetAvailableForBookingAsync(bookingId, ct));
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<EmployeeSimpleResponse>>> GetActiveEmployees(CancellationToken ct)
    {
        return Ok(await _userManager.GetActiveEmployeesAsync(ct));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.UsersEdit)]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await _userManager.GetAllUsersAsync(ct));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.UsersEdit)]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var requestingUserId = User.GetEmployeeId();
        if (requestingUserId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");

        return Ok(await _userManager.UpdateEmployeeAsync(id, request, requestingUserId.Value, ct));
    }

    [HttpPut("{id}/toggle")]
    [Authorize(Policy = PermissionKeys.UsersEdit)]
    public async Task<ActionResult<UserResponse>> Toggle(int id, CancellationToken ct)
    {
        var requestingUserId = User.GetEmployeeId();
        if (requestingUserId is null)
            return Problem(statusCode: 401, title: "INVALID_TOKEN", detail: "Invalid token.");

        return Ok(await _userManager.ToggleActiveAsync(id, requestingUserId.Value, ct));
    }
}