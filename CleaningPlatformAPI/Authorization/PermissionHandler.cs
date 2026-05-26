using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionHandler(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var stampClaim = context.User.FindFirst("security_stamp")?.Value;

        if (userIdClaim is null || stampClaim is null)
        {
            context.Fail();
            return;
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var authManager = scope.ServiceProvider.GetRequiredService<AuthManager>();
        var stampValid = await authManager.ValidateSecurityStampAsync(userId, stampClaim);

        if (!stampValid)
        {
            context.Fail();
            return;
        }

        // Owner bypasses all permission checks
        if (context.User.FindFirst(ClaimTypes.Role)?.Value == RoleNames.Owner)
        {
            context.Succeed(requirement);
            return;
        }

        // Check if user has the specific permission claim
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.PermissionKey);

        if (hasPermission)
            context.Succeed(requirement);
    }
}
