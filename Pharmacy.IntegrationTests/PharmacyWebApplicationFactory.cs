using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace Pharmacy.IntegrationTests;

/// <summary>
/// SQL Server integration host. Resolution order:
/// 1) Environment variable <c>PHARMACY_INTEGRATION_SQL</c> (full connection string).
/// 2) Testcontainers (Docker).
/// 3) Windows: ephemeral LocalDB database when Docker is unavailable.
/// </summary>
public sealed class PharmacyWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string EnvConnection = "PHARMACY_INTEGRATION_SQL";

    private readonly string _connectionString;
    private MsSqlContainer? _container;
    private string? _localDbName;

    public PharmacyWebApplicationFactory()
    {
        var fromEnv = Environment.GetEnvironmentVariable(EnvConnection);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            _connectionString = fromEnv.Trim();
            RunMigrations(_connectionString);
            return;
        }

        try
        {
            _container = new MsSqlBuilder().Build();
            _container.StartAsync().GetAwaiter().GetResult();
            _connectionString = _container.GetConnectionString();
            RunMigrations(_connectionString);
            return;
        }
        catch (ArgumentException)
        {
            if (!OperatingSystem.IsWindows())
                throw;

            _localDbName = "PharmacyIntegration_" + Guid.NewGuid().ToString("N");
            const string master =
                "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

            using (var conn = new SqlConnection(master))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE [{_localDbName}]";
                cmd.ExecuteNonQuery();
            }

            _connectionString =
                $"Server=(localdb)\\mssqllocaldb;Database={_localDbName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
            RunMigrations(_connectionString);
        }
    }

    private static void RunMigrations(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var db = new AppDbContext(options);
        db.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        if (_container is not null)
        {
            _container.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _container = null;
        }
        else if (_localDbName is not null)
        {
            try
            {
                const string master =
                    "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
                SqlConnection.ClearAllPools();
                using var conn = new SqlConnection(master);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText =
                    $"ALTER DATABASE [{_localDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_localDbName}]";
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // best-effort cleanup
            }

            _localDbName = null;
        }
    }
}
