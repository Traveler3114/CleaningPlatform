using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class ScheduleManagerTests : TestBase
{
    [Fact]
    public async Task GetScheduleAsync_ReturnsSchedule()
    {
        using var db = CreateDbContext();
        var manager = new ScheduleManager(db);

        var result = await manager.GetScheduleAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAndUpdateDay_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new ScheduleManager(db);

        var createResult = await manager.CreateDayAsync(new WeeklyScheduleRequest
        {
            DayOfWeek = 5,
            StartHour = 8,
            EndHour = 16,
            Capacity = 3
        });

        createResult.Success.Should().BeTrue();

        var updateResult = await manager.UpdateDayAsync(5, new UpdateWeeklyScheduleRequest
        {
            StartHour = 9,
            EndHour = 15,
            Capacity = 4
        });

        updateResult.Success.Should().BeTrue();
    }
}
