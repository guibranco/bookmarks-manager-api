using BookmarksManager.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Tests.Support;

/// <summary>
/// Owns an isolated, open in-memory SQLite connection for a single test, so each test class
/// instance gets its own database (xUnit creates a fresh instance per test method).
/// </summary>
public sealed class SqliteDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Db { get; }

    public SqliteDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
