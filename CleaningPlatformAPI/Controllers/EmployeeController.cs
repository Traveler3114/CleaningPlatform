using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeManager _userManager;

    public EmployeeController(EmployeeManager userManager) { _userManager = userManager; }

    [HttpGet("me")]
    public async Task<ActionResult<OperationResult<UserResponse>>> Me(CancellationToken ct)
    {
        var userId = User.GetEmployeeId();
        if (userId is null)
            return Unauthorized(OperationResult<UserResponse>.Fail("Invalid token."));

        var result = await _userManager.GetByIdAsync(userId.Value, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpGet("available-for-booking/{bookingId:int}")]
    public async Task<ActionResult<OperationResult<List<AvailableEmployeeResponse>>>> GetAvailableForBooking(int bookingId, CancellationToken ct)
    {
        var employees = await _userManager.GetAvailableForBookingAsync(bookingId, ct);
        return Ok(OperationResult<List<AvailableEmployeeResponse>>.Ok(employees));
    }

    [HttpGet("active")]
    public async Task<ActionResult<OperationResult<List<EmployeeSimpleResponse>>>> GetActiveEmployees(CancellationToken ct)
    {
        var employees = await _userManager.GetActiveEmployeesAsync(ct);
        return Ok(OperationResult<List<EmployeeSimpleResponse>>.Ok(employees));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.UsersEdit)]
    public async Task<ActionResult<OperationResult<List<UserResponse>>>> GetAll(CancellationToken ct)
    {
        var users = await _userManager.GetAllUsersAsync(ct);
        return Ok(OperationResult<List<UserResponse>>.Ok(users));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.UsersEdit)]
    public async Task<ActionResult<OperationResult<UserResponse>>> Update(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var requestingUserId = User.GetEmployeeId();
        if (requestingUserId is null)
            return Unauthorized(OperationResult<UserResponse>.Fail("Invalid token."));

        var result = await _userManager.UpdateEmployeeAsync(id, request, requestingUserId.Value, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id}/toggle")]
    [Authorize(Policy = PermissionKeys.UsersEdit)]
    public async Task<ActionResult<OperationResult<UserResponse>>> Toggle(int id, CancellationToken ct)
    {
        var requestingUserId = User.GetEmployeeId();
        if (requestingUserId is null)
            return Unauthorized(OperationResult<UserResponse>.Fail("Invalid token."));

        var result = await _userManager.ToggleActiveAsync(id, requestingUserId.Value, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}