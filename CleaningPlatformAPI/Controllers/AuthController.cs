using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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

    [Authorize(Policy = "actions.user.create")]
    [HttpPost("register")]
    public async Task<OperationResult<string>> Register(CreateUserDto request)
    {
        return await _authManager.RegisterAsync(request);
    }

    [HttpPost("login")]
    public async Task<OperationResult<string>> Login(LoginDto request)
    {
        return await _authManager.LoginAsync(request);
    }
}