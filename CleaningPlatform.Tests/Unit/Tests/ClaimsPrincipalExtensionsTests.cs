using System.Security.Claims;
using CleaningPlatformAPI.Extensions;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Unit.Tests;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "test"));

    [Fact]
    public void GetUserId_ReturnsParsedSubClaim()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "99"));
        var id = principal.GetUserId();
        id.Should().Be(99);
    }

    [Fact]
    public void GetUserId_MissingClaim_ReturnsZero()
    {
        var principal = CreatePrincipal();
        var id = principal.GetUserId();
        id.Should().Be(0);
    }

    [Fact]
    public void GetUserId_InvalidInt_ReturnsZero()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "not-a-number"));
        var id = principal.GetUserId();
        id.Should().Be(0);
    }
}
