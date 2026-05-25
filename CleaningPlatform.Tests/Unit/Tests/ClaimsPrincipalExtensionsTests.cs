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
    public void GetEmployeeId_ReturnsParsedSubClaim()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "99"));
        var id = principal.GetEmployeeId();
        id.Should().Be(99);
    }

    [Fact]
    public void GetEmployeeId_MissingClaim_ReturnsNull()
    {
        var principal = CreatePrincipal();
        var id = principal.GetEmployeeId();
        id.Should().BeNull();
    }

    [Fact]
    public void GetEmployeeId_InvalidInt_ReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "not-a-number"));
        var id = principal.GetEmployeeId();
        id.Should().BeNull();
    }
}
