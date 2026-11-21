using Epms.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Epms.Tests.Helpers;

public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly List<AppDbContext> _contexts = new();

    private TestDbContextFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public static async Task<TestDbContextFactory> CreateAsync(bool seed = true)
    {
        var factory = new TestDbContextFactory();
        var db = factory.CreateContext();
        await db.Database.EnsureCreatedAsync();
        if (seed)
        {
            await DbSeeder.SeedAsync(db, TestData.AdminPassword, TestData.EmployeePassword, includeTestAccounts: true);
        }
        return factory;
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        var context = new AppDbContext(options);
        _contexts.Add(context);
        return context;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }
        _connection.Dispose();
    }
}
