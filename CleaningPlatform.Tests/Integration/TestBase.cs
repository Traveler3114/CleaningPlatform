using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CleaningPlatformAPI.Data;

namespace CleaningPlatform.Tests.Integration;

public abstract class TestBase : IDisposable
{
    private readonly string _connectionString;
    private readonly TransactionScope _scope;

    protected TestBase()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is missing from appsettings.Test.json");
        _scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    protected AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new AppDbContext(options);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
