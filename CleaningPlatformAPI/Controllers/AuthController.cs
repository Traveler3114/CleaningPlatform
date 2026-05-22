using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthManager _authManager;

    public AuthController(AuthManager authManager)
    {
        _authManager = authManager;
    }

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
}