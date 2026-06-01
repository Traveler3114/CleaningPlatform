using CleaningPlatformAPI;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Entities;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class AvailabilityManagerTests : TestBase
{
    [Fact]
    public async Task GetSlotsAsync_ReturnsSlots_WhenScheduleExists()
    {
        using var db = CreateDbContext();
        var manager = new AvailabilityManager(db, NullStringLocalizer<SharedResources>.Instance);

        var slots = await manager.GetSlotsAsync(new DateTime(2026, 5, 26));

        slots.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSlotsAsync_ReturnsEmpty_ForFullyClosedDateOverride()
    {
        using var db = CreateDbContext();
        var closedDate = new DateTime(2026, 6, 1);
        db.DateOverrides.Add(new DateOverride
        {
            Date = DateOnly.FromDateTime(closedDate),
            IsFullyClosed = true,
            StartHour = 0,
            EndHour = 0,
            Capacity = 0
        });
        await db.SaveChangesAsync();
        var manager = new AvailabilityManager(db, NullStringLocalizer<SharedResources>.Instance);

        var slots = await manager.GetSlotsAsync(closedDate);

        slots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSlotsAsync_ReturnsClosedSlot_WhenDayIsClosed()
    {
        using var db = CreateDbContext();
        var manager = new AvailabilityManager(db, NullStringLocalizer<SharedResources>.Instance);

        var slots = await manager.GetSlotsAsync(new DateTime(2026, 5, 31));

        slots.Should().HaveCount(1);
        slots[0].IsClosed.Should().BeTrue();
        slots[0].Hour.Should().Be(0);
    }
}
