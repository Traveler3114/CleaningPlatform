using CleaningPlatformAPI;
using CleaningPlatformAPI.Managers;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class PortalDataManagerTests : TestBase
{
    [Fact]
    public async Task GetDashboardAsync_ReturnsStats()
    {
        using var db = CreateDbContext();
        var manager = new PortalDataManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetDashboardAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBookingsAsync_ReturnsClientBookings()
    {
        using var db = CreateDbContext();
        var manager = new PortalDataManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetBookingsAsync(1, null);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInvoicesAsync_ReturnsClientInvoices()
    {
        using var db = CreateDbContext();
        var manager = new PortalDataManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetInvoicesAsync(1);

        result.Should().NotBeNull();
    }
}
