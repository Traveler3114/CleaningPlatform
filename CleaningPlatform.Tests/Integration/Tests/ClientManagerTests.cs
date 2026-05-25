using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class ClientManagerTests : TestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);

        var result = await manager.GetAllAsync(new PaginationParams { Page = 1, PageSize = 10 });

        result.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingClient_ReturnsClient()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);
        var first = await manager.GetAllAsync(new PaginationParams());

        var result = await manager.GetByIdAsync(first.Items[0].Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_MissingClient_ReturnsFail()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);

        var result = await manager.GetByIdAsync(-1);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_PersonClient_Succeeds()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);
        var unique = Guid.NewGuid().ToString("N")[..8];

        var result = await manager.CreateAsync(new CreateClientRequest
        {
            ClientName = $"Test Person {unique}",
            Type = "Person",
            PrimaryContactName = "Contact",
            PrimaryContactPhone = "+385991234567",
            PrimaryContactEmail = $"test{unique}@email.com"
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_BusinessClient_Succeeds()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);
        var unique = Guid.NewGuid().ToString("N")[..8];

        var result = await manager.CreateAsync(new CreateClientRequest
        {
            ClientName = $"Test Biz {unique}",
            Type = "Business",
            PrimaryContactName = "Biz Contact",
            PrimaryContactPhone = "+385991234568",
            PrimaryContactEmail = $"biz{unique}@email.com"
        });

        result.Success.Should().BeTrue();
        result.Data.Type.Should().Be("Business");
    }

    [Fact]
    public async Task CreateSiteAsync_AddsSiteToClient()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);
        var all = await manager.GetAllAsync(new PaginationParams());
        var clientId = all.Items[0].Id;

        var result = await manager.CreateSiteAsync(clientId, new UpsertSiteRequest
        {
            SiteName = "Test Site",
            Address = "123 Main St",
            City = "Zagreb"
        });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetSitesAsync_ReturnsSites()
    {
        using var db = CreateDbContext();
        var manager = new ClientManager(db);
        var all = await manager.GetAllAsync(new PaginationParams());
        var clientId = all.Items[0].Id;

        var sites = await manager.GetSitesAsync(clientId);

        sites.Should().NotBeNull();
    }
}
