using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<OperationResult<string>>> Register([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _authManager.RegisterAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<OperationResult<string>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authManager.LoginAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<OperationResult<string>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = User.GetEmployeeId();
        if (userId is null)
            return Unauthorized(OperationResult<string>.Fail("INVALID_TOKEN", _localizer["error_invalid_token"]));

        var result = await _authManager.ChangePasswordAsync(request, userId.Value, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPost("reset-password")]
    [Authorize(Policy = PermissionKeys.UsersEdit)] // Only admins can reset others' passwords
    public async Task<ActionResult<OperationResult<string>>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var result = await _authManager.ResetPasswordAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}