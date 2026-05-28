using System.Security.Claims;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Middleware;

public class SecurityStampValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityStampValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuthManager authManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var stampClaim = context.User.FindFirst("security_stamp")?.Value;

            if (userIdClaim is null || stampClaim is null ||
                !int.TryParse(userIdClaim, out var userId) ||
                !await authManager.ValidateSecurityStampAsync(userId, stampClaim))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(OperationResult<string>.Fail("Session is no longer valid."));
                return;
            }
        }

        await _next(context);
    }
}
