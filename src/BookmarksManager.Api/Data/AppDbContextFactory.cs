using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookmarksManager.Api.Data;

/// <summary>
/// Used by `dotnet ef migrations add` at design time. Reads the target provider from the
/// EF_PROVIDER env var (Sqlite|MariaDb) so migrations can be generated into the matching
/// per-provider migrations project. The connection string only needs to be well-formed enough
/// for the provider to build a model; it is never connected to at migration-authoring time.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("EF_PROVIDER") ?? DatabaseProvider.Sqlite;
        var connectionString = provider.Equals(DatabaseProvider.MariaDb, StringComparison.OrdinalIgnoreCase)
            ? "Server=localhost;Database=bookmarks_manager;User=root;Password=root;"
            : "Data Source=bookmarks-manager.design.db";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        DatabaseProvider.Configure(optionsBuilder, provider, connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
