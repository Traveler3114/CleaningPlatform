using CleaningPlatformAPI.Managers;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class KanbanManagerTests : TestBase
{
    [Fact]
    public async Task GetPipelineAsync_ReturnsPipeline()
    {
        using var db = CreateDbContext();
        var manager = new KanbanManager(db);

        var result = await manager.GetPipelineAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWeekAsync_ReturnsWeekView()
    {
        using var db = CreateDbContext();
        var manager = new KanbanManager(db);
        var monday = new DateTime(2026, 5, 25);

        var result = await manager.GetWeekAsync(monday);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetResourceGridAsync_ReturnsGrid()
    {
        using var db = CreateDbContext();
        var manager = new KanbanManager(db);

        var result = await manager.GetResourceGridAsync(new DateTime(2026, 5, 25), "week");

        result.Should().NotBeNull();
    }
}
