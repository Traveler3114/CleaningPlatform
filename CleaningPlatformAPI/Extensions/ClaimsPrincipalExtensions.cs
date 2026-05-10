using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CleaningPlatformAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string key)
    {
        if (user.FindFirst(ClaimTypes.Role)?.Value == "Owner") return true;
        return user.HasClaim("permission", key);
    }

    public static int? GetEmployeeId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user.FindFirst("sub")?.Value;

        return int.TryParse(raw, out var id) ? id : null;
    }
}
