using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class AuthManagerTests : TestBase
{
    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "this-is-a-test-key-that-is-at-least-32-characters-long!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:ExpiryHours"] = "24"
            })
            .Build();
    }

    private static TokenManager CreateTokenManager()
    {
        return new TokenManager(CreateConfiguration());
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        using var db = CreateDbContext();
        var tokenManager = CreateTokenManager();
        var manager = new AuthManager(tokenManager, db, CreateConfiguration());

        var result = await manager.LoginAsync(new LoginRequest { Username = "owner", Password = "ChangeMe123!" });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFail()
    {
        using var db = CreateDbContext();
        var tokenManager = CreateTokenManager();
        var manager = new AuthManager(tokenManager, db, CreateConfiguration());

        var result = await manager.LoginAsync(new LoginRequest { Username = "owner", Password = "wrongpassword" });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_NonexistentUser_ReturnsFail()
    {
        using var db = CreateDbContext();
        var tokenManager = CreateTokenManager();
        var manager = new AuthManager(tokenManager, db, CreateConfiguration());

        var result = await manager.LoginAsync(new LoginRequest { Username = "nobody", Password = "password" });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser()
    {
        using var db = CreateDbContext();
        var tokenManager = CreateTokenManager();
        var manager = new AuthManager(tokenManager, db, CreateConfiguration());

        var result = await manager.RegisterAsync(new CreateUserRequest
        {
            Password = "TestPass123!",
            FirstName = "Test",
            LastName = "User",
            Role = "Owner"
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFail()
    {
        using var db = CreateDbContext();
        var tokenManager = CreateTokenManager();
        var manager = new AuthManager(tokenManager, db, CreateConfiguration());

        await manager.RegisterAsync(new CreateUserRequest
        {
            Password = "TestPass123!",
            FirstName = "Duplicate",
            LastName = "Test",
            Role = "Employee"
        });

        for (var i = 0; i < 20; i++)
        {
            await manager.RegisterAsync(new CreateUserRequest
            {
                Password = "TestPass123!",
                FirstName = "Duplicate",
                LastName = "Test",
                Role = "Employee"
            });
        }

        var result = await manager.RegisterAsync(new CreateUserRequest
        {
            Password = "TestPass123!",
            FirstName = "Duplicate",
            LastName = "Test",
            Role = "Employee"
        });

        result.Success.Should().BeFalse();
    }
}
