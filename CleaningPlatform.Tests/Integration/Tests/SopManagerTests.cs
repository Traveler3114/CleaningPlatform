using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class SopManagerTests : TestBase
{
    [Fact]
    public async Task GetAllTemplatesAsync_ReturnsTemplates()
    {
        using var db = CreateDbContext();
        var manager = new SopManager(db);

        var result = await manager.GetAllTemplatesAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAndDeleteTemplate_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new SopManager(db);
        var unique = Guid.NewGuid().ToString("N")[..8];

        var createResult = await manager.CreateTemplateAsync(new CreateSopTemplateRequest
        {
            Name = $"Test SOP {unique}",
            ServiceType = "SiteBased",
            IsActive = true
        });

        createResult.Success.Should().BeTrue();
        createResult.Data.Should().NotBeNull();

        var deleteResult = await manager.DeleteTemplateAsync(createResult.Data.Id);

        deleteResult.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddAndDeleteChecklistItem_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new SopManager(db);
        var unique = Guid.NewGuid().ToString("N")[..8];

        var createResult = await manager.CreateTemplateAsync(new CreateSopTemplateRequest
        {
            Name = $"Checklist Test {unique}",
            ServiceType = "Vehicle",
            IsActive = true
        });

        var addResult = await manager.AddChecklistItemAsync(createResult.Data!.Id, new UpsertChecklistItemRequest
        {
            ItemText = "Test checklist item",
            SortOrder = 1,
            IsRequired = true
        });

        addResult.Success.Should().BeTrue();
        addResult.Data!.ItemText.Should().Be("Test checklist item");

        var deleteItemResult = await manager.DeleteChecklistItemAsync(addResult.Data.Id);

        deleteItemResult.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTemplateAsync_UpdatesFields()
    {
        using var db = CreateDbContext();
        var manager = new SopManager(db);
        var templates = await manager.GetAllTemplatesAsync();

        if (templates.Count == 0) return;

        var first = templates[0];

        var result = await manager.UpdateTemplateAsync(first.Id, new CreateSopTemplateRequest
        {
            Name = first.Name,
            ServiceType = first.ServiceType,
            IsActive = first.IsActive
        });

        result.Success.Should().BeTrue();
    }
}
