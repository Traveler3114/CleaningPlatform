using CleaningPlatformAPI;
using CleaningPlatformAPI.Managers;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class EmployeeManagerTests : TestBase
{
    [Fact]
    public async Task GetAllUsersAsync_ReturnsUsers()
    {
        using var db = CreateDbContext();
        var manager = new EmployeeManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetAllUsersAsync();

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetActiveEmployeesAsync_ReturnsOnlyActive()
    {
        using var db = CreateDbContext();
        var manager = new EmployeeManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetActiveEmployeesAsync();

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        using var db = CreateDbContext();
        var manager = new EmployeeManager(db, NullStringLocalizer<SharedResources>.Instance);
        var all = await manager.GetAllUsersAsync();
        var firstId = all[0].Id;

        var result = await manager.GetByIdAsync(firstId);

        result.Should().NotBeNull();
        result.Data!.Id.Should().Be(firstId);
    }

    [Fact]
    public async Task ToggleActiveAsync_SwitchesStatus()
    {
        using var db = CreateDbContext();
        var manager = new EmployeeManager(db, NullStringLocalizer<SharedResources>.Instance);
        var all = await manager.GetAllUsersAsync();
        all.Should().HaveCountGreaterThan(1, "need at least two users to test actor/target separation");

        var target = all[0];
        var actor = all.First(u => u.Id != target.Id);

        var result = await manager.ToggleActiveAsync(target.Id, actor.Id);

        result.Success.Should().BeTrue();
    }
}
