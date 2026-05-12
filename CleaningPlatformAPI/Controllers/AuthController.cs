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

    [Authorize(Policy = PermissionKeys.ActionsUserCreate)]
    [HttpPost("register")]
    public async Task<OperationResult<string>> Register([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        return await _authManager.RegisterAsync(request, ct);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<OperationResult<string>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        return await _authManager.LoginAsync(request, ct);
    }
}
