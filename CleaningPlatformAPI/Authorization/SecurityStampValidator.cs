using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Authorization;

public class SecurityStampValidator
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityStampValidator> _logger;

    public SecurityStampValidator(RequestDelegate next, ILogger<SecurityStampValidator> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.GetEmployeeId();
        var claimStamp = context.User.FindFirst("security_stamp")?.Value;
        if (!userId.HasValue || string.IsNullOrWhiteSpace(claimStamp))
        {
            _logger.LogWarning("Rejecting authenticated request due to missing user id or security_stamp claim.");
            await RejectAsync(context);
            return;
        }

        var currentStamp = await db.Employees
            .Where(e => e.Id == userId.Value)
            .Select(e => e.SecurityStamp)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(currentStamp) || !string.Equals(claimStamp, currentStamp, StringComparison.Ordinal))
        {
            _logger.LogWarning("Rejecting request due to security stamp mismatch or missing DB stamp for user {UserId}.", userId.Value);
            await RejectAsync(context);
            return;
        }

        await _next(context);
    }

    private static async Task RejectAsync(HttpContext context)
    {
        var isApiRequest = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
        if (isApiRequest)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Redirect("/Admin/Login");
    }
}
