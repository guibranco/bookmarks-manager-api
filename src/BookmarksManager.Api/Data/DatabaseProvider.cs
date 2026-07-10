using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Data;

public static class DatabaseProvider
{
    public const string Sqlite = "Sqlite";
    public const string MariaDb = "MariaDb";

    public const string SqliteMigrationsAssembly = "BookmarksManager.Migrations.Sqlite";
    public const string MariaDbMigrationsAssembly = "BookmarksManager.Migrations.MariaDb";

    /// <summary>Baseline MariaDB version assumed when none is configured. Avoids an AutoDetect
    /// round-trip to the server at startup, which would make app start depend on DB availability.</summary>
    public const string DefaultMariaDbServerVersion = "10.11.6-mariadb";

    public static void Configure(
        DbContextOptionsBuilder options,
        string provider,
        string connectionString,
        string? mariaDbServerVersion = null)
    {
        if (provider.Equals(MariaDb, StringComparison.OrdinalIgnoreCase))
        {
            var serverVersion = ServerVersion.Parse(mariaDbServerVersion ?? DefaultMariaDbServerVersion);
            options.UseMySql(
                connectionString,
                serverVersion,
                b => b.MigrationsAssembly(MariaDbMigrationsAssembly));
        }
        else if (provider.Equals(Sqlite, StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(
                connectionString,
                b => b.MigrationsAssembly(SqliteMigrationsAssembly));
        }
        else
        {
            throw new NotSupportedException(
                $"Database provider '{provider}' is not supported. Use '{Sqlite}' or '{MariaDb}'.");
        }
    }
}
