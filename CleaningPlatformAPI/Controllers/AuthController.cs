using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthManager _authManager;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AuthController(AuthManager authManager, IStringLocalizer<SharedResources> localizer) { _authManager = authManager; 
            _localizer = localizer;}

    [Authorize(Policy = PermissionKeys.UsersCreate)]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        await _authManager.RegisterAsync(request, ct);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        return Ok(await _authManager.LoginAsync(request, ct));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = User.RequireEmployeeId();
        await _authManager.ChangePasswordAsync(request, userId, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [Authorize(Policy = PermissionKeys.UsersEdit)] // Only admins can reset others' passwords
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        await _authManager.ResetPasswordAsync(request, ct);
        return NoContent();
    }
}