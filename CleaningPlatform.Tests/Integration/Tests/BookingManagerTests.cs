using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Common;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class BookingManagerTests : TestBase
{
    [Fact]
    public async Task GetAllBookingsAsync_ReturnsPagedResults()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);

        var result = await manager.GetAllBookingsAsync(new PaginationParams { Page = 1, PageSize = 10 });

        result.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetBookingDetailByIdAsync_ExistingBooking_ReturnsDetail()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);
        var first = await manager.GetAllBookingsAsync(new PaginationParams { Page = 1, PageSize = 1 });

        var result = await manager.GetBookingDetailByIdAsync(first.Items[0].Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBookingDetailByIdAsync_MissingBooking_ReturnsFail()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);

        var result = await manager.GetBookingDetailByIdAsync(-1);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_Succeeds()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);
        var first = await manager.GetAllBookingsAsync(new PaginationParams { Page = 1, PageSize = 1 });
        var bookingId = first.Items[0].Id;

        var result = await manager.UpdateStatusAsync(bookingId, "Confirmed");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidTransition_ReturnsFail()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);
        var first = await manager.GetAllBookingsAsync(new PaginationParams { Page = 1, PageSize = 1 });
        var bookingId = first.Items[0].Id;

        await manager.UpdateStatusAsync(bookingId, "Cancelled");
        var result = await manager.UpdateStatusAsync(bookingId, "Confirmed");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetBookingsAsync_ByDate_ReturnsBookings()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);
        var today = DateTime.Today;

        var result = await manager.GetBookingsAsync(today);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAssignmentAsync_AddsEmployeeToBooking()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);
        var booking = await db.Bookings
            .Where(b => b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled)
            .FirstAsync();
        var bookingId = booking.Id;

        var assignedIds = await db.BookingAssignments
            .Where(ba => ba.BookingId == bookingId)
            .Select(ba => ba.EmployeeId)
            .ToListAsync();
        var employee = await db.Employees
            .FirstAsync(e => !assignedIds.Contains(e.Id));

        var result = await manager.AddAssignmentAsync(bookingId, employee.Id);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllBookingsAsync_StatusFilter_FiltersCorrectly()
    {
        using var db = CreateDbContext();
        var availability = new AvailabilityManager(db);
        var sop = new SopManager(db);
        var manager = new BookingManager(db, availability, sop);

        var result = await manager.GetAllBookingsAsync(new PaginationParams(), "Pending");

        result.Items.All(b => b.Status == "Pending").Should().BeTrue();
    }
}
