using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CleaningPlatformAPI.Managers;

public class TokenManager
{
    private readonly IConfiguration _config;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TokenManager(IConfiguration config, IStringLocalizer<SharedResources> localizer) { _config = config; 
            _localizer = localizer;}

    private (SymmetricSecurityKey Key, SigningCredentials Creds, string Issuer) GetSigningConfig()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        return (key, creds, _config["Jwt:Issuer"]!);
    }

    public string CreateAdminToken(Employee user, List<string> permissions)
    {
        var (key, creds, issuer) = GetSigningConfig();
        var expiry = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!));

        var roleName = user.Role?.Name ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("security_stamp", user.SecurityStamp),
            new Claim("auth_type", "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (roleName != RoleNames.Owner)
        {
            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));
        }

        return CreateToken(claims, expiry, issuer, creds);
    }

    public string CreateMagicLinkToken(int clientId, string email, string name)
    {
        var (key, creds, issuer) = GetSigningConfig();
        var claims = new List<Claim>
        {
            new Claim("client_id", clientId.ToString()),
            new Claim("email", email),
            new Claim("name", name),
            new Claim("auth_type", "portal"),
            new Claim("purpose", "magic_link"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims, DateTime.UtcNow.AddMinutes(15), issuer, creds);
    }

    public string CreatePortalSessionToken(int clientId, string email, string name)
    {
        var (key, creds, issuer) = GetSigningConfig();
        var claims = new List<Claim>
        {
            new Claim("client_id", clientId.ToString()),
            new Claim("email", email),
            new Claim("name", name),
            new Claim("auth_type", "portal"),
            new Claim(ClaimTypes.Role, "Client"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiry = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!));
        return CreateToken(claims, expiry, issuer, creds);
    }

    public string CreateBookingRequestToken(int bookingRequestId, string email)
    {
        var (key, creds, issuer) = GetSigningConfig();
        var claims = new List<Claim>
        {
            new Claim("booking_request_id", bookingRequestId.ToString()),
            new Claim("email", email),
            new Claim("purpose", "booking_request_confirmation"),
            new Claim("auth_type", "public"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims, DateTime.UtcNow.AddHours(48), issuer, creds);
    }

    public ClaimsIdentity? ValidateBookingRequestToken(string token)
    {
        try
        {
            var (key, _, issuer) = GetSigningConfig();
            var handler = new JsonWebTokenHandler();
            var result = handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }).GetAwaiter().GetResult();

            return result.IsValid ? result.ClaimsIdentity : null;
        }
        catch
        {
            return null;
        }
    }

    private static string CreateToken(List<Claim> claims, DateTime expires, string issuer, SigningCredentials creds)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            SigningCredentials = creds
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
