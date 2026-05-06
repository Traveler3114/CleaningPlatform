using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthManager _authManager;

    public AuthController(AuthManager authManager)
    {
        _authManager = authManager;
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("register")]
    public async Task<ActionResult<OperationResult<string>>> Register(CreateUserDto request)
    {
        var result = await _authManager.RegisterAsync(request);
        if(!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<ActionResult<OperationResult<string>>> Login(LoginDto request)
    {
        var result = await _authManager.LoginAsync(request);
        if (!result.Success)
            return Unauthorized(result);
        return Ok(result);
    }
}
