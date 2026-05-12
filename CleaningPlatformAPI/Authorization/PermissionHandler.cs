using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Owner bypasses all permission checks
        if (context.User.FindFirst(ClaimTypes.Role)?.Value == RoleNames.Owner)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has the specific permission claim
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.PermissionKey);

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
