using System.Security.Claims;
using System.Text;
using CleaningPlatformAPI.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CleaningPlatformAPI.Managers;

public class TokenManager
{
    private readonly IConfiguration _config;

    public TokenManager(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user, List<string> permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, user.RoleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Owner gets all permissions via role claim; others get explicit permission claims
        if (user.RoleName != "Owner")
        {
            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            Issuer = _config["Jwt:Issuer"],
            SigningCredentials = creds
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}
