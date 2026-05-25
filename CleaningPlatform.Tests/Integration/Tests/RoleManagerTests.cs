using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class RoleManagerTests : TestBase
{
    [Fact]
    public async Task GetAllRolesAsync_ReturnsRoles()
    {
        using var db = CreateDbContext();
        var manager = new RoleManager(db);

        var result = await manager.GetAllRolesAsync();

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAndDeleteRole_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new RoleManager(db);

        var createResult = await manager.CreateRoleAsync(new CreateRoleRequest
        {
            Name = $"TestRole_{Guid.NewGuid():N}",
            Permissions = new List<string> { "test.permission" }
        });

        createResult.Success.Should().BeTrue();
    }

    [Fact]
    public void GetAvailablePermissions_ReturnsNonEmpty()
    {
        using var db = CreateDbContext();
        var manager = new RoleManager(db);

        var result = manager.GetAvailablePermissions();

        result.Should().NotBeEmpty();
    }
}
