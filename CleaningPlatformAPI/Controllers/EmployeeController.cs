using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
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
    public async Task<OperationResult<UserResponse>> Me(CancellationToken ct)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(sub, out var userId))
            return OperationResult<UserResponse>.Fail("Invalid token.");

        var user = await _userManager.GetByIdAsync(userId, ct);
        return user == null
            ? OperationResult<UserResponse>.Fail("User not found.")
            : OperationResult<UserResponse>.Ok(user);
    }

    [HttpGet("/api/employees")]
    public async Task<OperationResult<List<EmployeeSimpleResponse>>> GetActiveEmployees(CancellationToken ct)
    {
        return OperationResult<List<EmployeeSimpleResponse>>.Ok(await _userManager.GetActiveEmployeesAsync(ct));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.ActionsUserToggleActive)]
    public async Task<OperationResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        return OperationResult<List<UserResponse>>.Ok(await _userManager.GetAllUsersAsync(ct));
    }

    [HttpPut("{id}/toggle")]
    [Authorize(Policy = PermissionKeys.ActionsUserToggleActive)]
    public async Task<OperationResult<UserResponse>> Toggle(int id, CancellationToken ct)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(sub, out var requestingUserId))
            return OperationResult<UserResponse>.Fail("Invalid token.");

        return await _userManager.ToggleActiveAsync(id, requestingUserId, ct);
    }
}
