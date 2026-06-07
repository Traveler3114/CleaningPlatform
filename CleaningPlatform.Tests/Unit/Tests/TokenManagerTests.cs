using System.Security.Claims;
using System.Text;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Managers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CleaningPlatform.Tests.Unit.Tests;

public class TokenManagerTests
{
    private static IConfiguration CreateConfig(string key, string issuer, string expiryHours = "24")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = issuer,
                ["Jwt:ExpiryHours"] = expiryHours
            })
            .Build();
    }

    [Fact]
    public async Task CreateAdminToken_ReturnsValidJwt_WithCorrectClaims()
    {
        var key = "this-is-a-test-key-that-is-at-least-32-characters-long!";
        var issuer = "TestIssuer";
        var config = CreateConfig(key, issuer);
        var manager = new TokenManager(config, NullStringLocalizer<SharedResources>.Instance);
        var user = new Employee
        {
            Id = 42,
            Username = "testuser",
            SecurityStamp = "stamp123",
            Role = new Role { Name = "Manager" }
        };
        List<string> permissions = ["bookings.read", "bookings.write"];

        var token = manager.CreateAdminToken(user, permissions);
        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = false,
            ValidateLifetime = true
        });

        result.IsValid.Should().BeTrue();
        result.Claims["sub"].Should().Be("42");
        result.Claims[ClaimTypes.Name].Should().Be("testuser");
        result.Claims[ClaimTypes.Role].Should().Be("Manager");
        result.Claims["security_stamp"].Should().Be("stamp123");
        result.Claims["auth_type"].Should().Be("admin");
        result.Claims.Should().ContainKey(JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public async Task CreateAdminToken_OwnerRole_OmitsPermissions()
    {
        var config = CreateConfig("test-key-at-least-32-characters-for-testing!", "Issuer");
        var manager = new TokenManager(config, NullStringLocalizer<SharedResources>.Instance);
        var user = new Employee
        {
            Id = 1,
            Username = "owner",
            SecurityStamp = "abc",
            Role = new Role { Name = RoleNames.Owner }
        };

        var token = manager.CreateAdminToken(user, ["some.permission"]);
        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key-at-least-32-characters-for-testing!")),
            ValidateIssuer = true,
            ValidIssuer = "Issuer",
            ValidateAudience = false,
            ValidateLifetime = true
        });

        result.Claims[ClaimTypes.Role].Should().Be(RoleNames.Owner);
        result.Claims.Should().NotContainKey("permission");
    }

    [Fact]
    public async Task CreateMagicLinkToken_ContainsCorrectClaims()
    {
        var config = CreateConfig("test-key-at-least-32-characters-for-testing!", "CP");
        var manager = new TokenManager(config, NullStringLocalizer<SharedResources>.Instance);

        var token = manager.CreateMagicLinkToken(7, "test@email.com", "Test User");
        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key-at-least-32-characters-for-testing!")),
            ValidateIssuer = true,
            ValidIssuer = "CP",
            ValidateAudience = false,
            ValidateLifetime = true
        });

        result.Claims["client_id"].Should().Be("7");
        result.Claims["email"].Should().Be("test@email.com");
        result.Claims["name"].Should().Be("Test User");
        result.Claims["auth_type"].Should().Be("portal");
        result.Claims["purpose"].Should().Be("magic_link");
    }

    [Fact]
    public async Task CreatePortalSessionToken_ContainsCorrectClaims()
    {
        var config = CreateConfig("test-key-at-least-32-characters-for-testing!", "CP");
        var manager = new TokenManager(config, NullStringLocalizer<SharedResources>.Instance);

        var token = manager.CreatePortalSessionToken(3, "portal@test.com", "Portal User");
        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key-at-least-32-characters-for-testing!")),
            ValidateIssuer = true,
            ValidIssuer = "CP",
            ValidateAudience = false,
            ValidateLifetime = true
        });

        result.Claims["client_id"].Should().Be("3");
        result.Claims["email"].Should().Be("portal@test.com");
        result.Claims["name"].Should().Be("Portal User");
        result.Claims["auth_type"].Should().Be("portal");
        result.Claims[ClaimTypes.Role].Should().Be("Client");
    }

    [Fact]
    public void Token_Expiry_RespectsConfiguredHours()
    {
        var config = CreateConfig("test-key-at-least-32-characters-for-testing!", "Issuer", "1");
        var manager = new TokenManager(config, NullStringLocalizer<SharedResources>.Instance);
        var user = new Employee
        {
            Id = 1,
            Username = "u",
            SecurityStamp = "s",
            Role = new Role { Name = "User" }
        };

        var token = manager.CreateAdminToken(user, []);
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(2));
    }
}
