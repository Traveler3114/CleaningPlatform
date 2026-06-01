using CleaningPlatformAPI;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class RecurringScheduleManagerTests : TestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsSchedules()
    {
        using var db = CreateDbContext();
        var sop = new SopManager(db, NullStringLocalizer<SharedResources>.Instance);
        var manager = new RecurringScheduleManager(db, sop, NullLogger<RecurringScheduleManager>.Instance, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetAllAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAutoGenerateAsync_CompletesSuccessfully()
    {
        using var db = CreateDbContext();
        var sop = new SopManager(db, NullStringLocalizer<SharedResources>.Instance);
        var manager = new RecurringScheduleManager(db, sop, NullLogger<RecurringScheduleManager>.Instance, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.RunAutoGenerateAsync();

        result.Should().NotBeNull();
    }
}
