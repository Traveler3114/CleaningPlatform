using CleaningPlatformAPI;
using CleaningPlatformAPI.Managers;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class ReportingManagerTests : TestBase
{
    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsSummary()
    {
        using var db = CreateDbContext();
        var manager = new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetDashboardSummaryAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMonthlyRevenueAsync_ReturnsRevenue()
    {
        using var db = CreateDbContext();
        var manager = new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetMonthlyRevenueAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTopClientsAsync_ReturnsClients()
    {
        using var db = CreateDbContext();
        var manager = new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetTopClientsAsync(5);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEmployeeUtilizationAsync_ReturnsUtilization()
    {
        using var db = CreateDbContext();
        var manager = new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetEmployeeUtilizationAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetJobCompletionRateAsync_ReturnsRates()
    {
        using var db = CreateDbContext();
        var manager = new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetJobCompletionRateAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOverdueInvoiceSummaryAsync_ReturnsSummary()
    {
        using var db = CreateDbContext();
        var manager = new ReportingManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetOverdueInvoiceSummaryAsync();

        result.Should().NotBeNull();
    }
}
