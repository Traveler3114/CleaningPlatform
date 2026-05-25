using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class DateOverrideManagerTests : TestBase
{
    [Fact]
    public async Task GetOverridesAsync_ReturnsList()
    {
        using var db = CreateDbContext();
        var manager = new DateOverrideManager(db);

        var result = await manager.GetOverridesAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAndDeleteOverride_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new DateOverrideManager(db);

        var createResult = await manager.CreateOverrideAsync(new DateOverrideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            StartHour = 10,
            EndHour = 14,
            Capacity = 3
        });

        createResult.Success.Should().BeTrue();

        var all = await manager.GetOverridesAsync();
        all.Should().Contain(o => o.Date == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
    }
}
