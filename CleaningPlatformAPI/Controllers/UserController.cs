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
public class UserController : ControllerBase
{
    private readonly UserManager _userManager;

    public UserController(UserManager userManager)
    {
        _userManager = userManager;
    }

    // GET /api/users/me — returns current user profile with permissions
    [HttpGet("me")]
    public async Task<ActionResult<OperationResult<UserDto>>> Me()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(sub, out var userId))
            return Unauthorized(OperationResult<UserDto>.Fail("Invalid token."));

        var user = await _userManager.GetByIdAsync(userId);
        if (user == null)
            return NotFound(OperationResult<UserDto>.Fail("User not found."));

        return Ok(OperationResult<UserDto>.Ok(user));
    }

    // GET /api/users — list all users
    [HttpGet]
    [Authorize(Policy = "actions.user.toggleActive")]
    public async Task<ActionResult<OperationResult<List<UserDto>>>> GetAll()
    {
        var users = await _userManager.GetAllUsersAsync();
        return Ok(OperationResult<List<UserDto>>.Ok(users));
    }

    // PUT /api/users/{id}/toggle — toggle user active status
    [HttpPut("{id}/toggle")]
    [Authorize(Policy = "actions.user.toggleActive")]
    public async Task<ActionResult<OperationResult<UserDto>>> Toggle(int id)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(sub, out var requestingUserId))
            return Unauthorized(OperationResult<UserDto>.Fail("Invalid token."));

        var result = await _userManager.ToggleActiveAsync(id, requestingUserId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
