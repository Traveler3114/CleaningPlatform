using System.Security.Claims;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string key)
    {
        if (user.FindFirst(ClaimTypes.Role)?.Value == RoleNames.Owner) return true;
        return user.HasClaim("permission", key);
    }

    public static int? GetEmployeeId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(raw, out var id) ? id : null;
    }
}
