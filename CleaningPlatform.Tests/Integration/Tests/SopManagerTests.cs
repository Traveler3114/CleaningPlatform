using CleaningPlatformAPI;
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
        var manager = new SopManager(db, NullStringLocalizer<SharedResources>.Instance);

        var result = await manager.GetAllTemplatesAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAndToggleTemplate_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new SopManager(db, NullStringLocalizer<SharedResources>.Instance);
        var unique = Guid.NewGuid().ToString("N")[..8];

        var createResult = await manager.CreateTemplateAsync(new CreateSopTemplateRequest
        {
            Name = $"Test SOP {unique}",
            ServiceType = "SiteBased",
            IsActive = true
        });

        createResult.Success.Should().BeTrue();
        createResult.Data.Should().NotBeNull();

        var toggleResult = await manager.ToggleActiveAsync(createResult.Data.Id, false);

        toggleResult.Success.Should().BeTrue();
        toggleResult.Data!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AddAndDeleteChecklistItem_Workflow()
    {
        using var db = CreateDbContext();
        var manager = new SopManager(db, NullStringLocalizer<SharedResources>.Instance);
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
        var manager = new SopManager(db, NullStringLocalizer<SharedResources>.Instance);
        var templates = await manager.GetAllTemplatesAsync();

        templates.Should().NotBeEmpty("seed data must exist for this test to be meaningful");

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
