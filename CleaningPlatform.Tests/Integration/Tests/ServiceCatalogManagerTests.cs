using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class ServiceCatalogManagerTests : TestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsServices()
    {
        using var db = CreateDbContext();
        var manager = new ServiceCatalogManager(db);

        var result = await manager.GetAllAsync();

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUpdateDelete_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new ServiceCatalogManager(db);
        var unique = Guid.NewGuid().ToString("N")[..8];

        var createResult = await manager.CreateAsync(new ServiceCatalogUpsertRequest
        {
            CatalogCode = $"T-{unique}",
            Name = $"Test Service {unique}",
            Category = "Cleaning",
            Unit = "hour",
            ServiceType = "SiteBased",
            PriceMin = 10,
            PriceMax = 50,
            PriceAvg = 30
        });

        createResult.Success.Should().BeTrue();

        var all = await manager.GetAllAsync();
        all.Should().Contain(s => s.Name.Contains(unique));
    }
}
