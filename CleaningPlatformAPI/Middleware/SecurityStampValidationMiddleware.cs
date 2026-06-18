using System.Security.Claims;
using CleaningPlatformAPI.Managers;
using Microsoft.AspNetCore.Mvc;

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
            var authType = context.User.FindFirst("auth_type")?.Value;

            // Portal tokens are validated via magic-link exchange — no security stamp to check
            if (authType != "portal")
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var stampClaim = context.User.FindFirst("security_stamp")?.Value;

                if (userIdClaim is null || stampClaim is null ||
                    !int.TryParse(userIdClaim, out var userId) ||
                    !await authManager.ValidateSecurityStampAsync(userId, stampClaim))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/problem+json";
                    var pd = new ProblemDetails
                    {
                        Type = "https://httpstatuses.com/401",
                        Title = "Unauthorized",
                        Status = 401,
                        Detail = "Session is no longer valid.",
                        Extensions = { ["code"] = "SESSION_INVALID" }
                    };
                    await context.Response.WriteAsJsonAsync(pd, pd.GetType());
                    return;
                }
            }
        }

        await _next(context);
    }
}
