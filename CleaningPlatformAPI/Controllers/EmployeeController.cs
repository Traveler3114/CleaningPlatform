using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeManager _userManager;

    public EmployeeController(EmployeeManager userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<OperationResult<UserDto>> Me()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(sub, out var userId))
            return OperationResult<UserDto>.Fail("Invalid token.");

        var user = await _userManager.GetByIdAsync(userId);
        if (user == null)
            return OperationResult<UserDto>.Fail("User not found.");

        return OperationResult<UserDto>.Ok(user);
    }

    [HttpGet("/api/employees")]
    public async Task<OperationResult<List<EmployeeSimpleDto>>> GetActiveEmployees()
    {
        var employees = await _userManager.GetActiveEmployeesAsync();
        return OperationResult<List<EmployeeSimpleDto>>.Ok(employees);
    }

    [HttpGet]
    [Authorize(Policy = "actions.user.toggleActive")]
    public async Task<OperationResult<List<UserDto>>> GetAll()
    {
        var users = await _userManager.GetAllUsersAsync();
        return OperationResult<List<UserDto>>.Ok(users);
    }

    [HttpPut("{id}/toggle")]
    [Authorize(Policy = "actions.user.toggleActive")]
    public async Task<OperationResult<UserDto>> Toggle(int id)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(sub, out var requestingUserId))
            return OperationResult<UserDto>.Fail("Invalid token.");

        return await _userManager.ToggleActiveAsync(id, requestingUserId);
    }
}