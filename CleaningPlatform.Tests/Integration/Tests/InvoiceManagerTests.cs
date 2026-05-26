using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CleaningPlatform.Tests.Integration.Tests;

public class InvoiceManagerTests : TestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        using var db = CreateDbContext();
        var manager = new InvoiceManager(db);

        var result = await manager.GetAllAsync(new PaginationParams());

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingInvoice_ReturnsDetail()
    {
        using var db = CreateDbContext();
        var manager = new InvoiceManager(db);
        var all = await manager.GetAllAsync(new PaginationParams());

        all.Items.Should().NotBeEmpty("seed data must exist for this test to be meaningful");

        var result = await manager.GetByIdAsync(all.Items[0].Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_MissingInvoice_ReturnsFail()
    {
        using var db = CreateDbContext();
        var manager = new InvoiceManager(db);

        var result = await manager.GetByIdAsync(-1);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RecordPaymentAsync_AddsPayment()
    {
        using var db = CreateDbContext();
        var manager = new InvoiceManager(db);
        var all = await manager.GetAllAsync(new PaginationParams());

        all.Items.Should().NotBeEmpty("seed data must exist for this test to be meaningful");

        var invoiceId = all.Items[0].Id;
        var employee = await db.Employees.FirstAsync();

        var result = await manager.RecordPaymentAsync(invoiceId, new RecordPaymentRequest
        {
            Amount = 50,
            Method = "BankTransfer"
        }, employee.Id);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_Succeeds()
    {
        using var db = CreateDbContext();
        var manager = new InvoiceManager(db);
        var all = await manager.GetAllAsync(new PaginationParams());

        all.Items.Should().NotBeEmpty("seed data must exist for this test to be meaningful");

        var result = await manager.UpdateStatusAsync(all.Items[0].Id, "Sent");

        result.Success.Should().BeTrue();
    }
}
