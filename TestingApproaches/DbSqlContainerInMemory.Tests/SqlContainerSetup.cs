using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace DbSqlContainerInMemory.Tests;

[SetUpFixture]
public class SqlContainerSetup
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";

    // Matches db-dev.sh. A long-lived container started once for the local
    // dev loop; when it's reachable we connect straight to it instead of
    // paying the SQL Server boot cost on every `dotnet test` run. CI has no
    // such container running, so it falls through to the ephemeral one below.
    private const string PersistentHost = "localhost";
    private const int PersistentPort = 14330;
    private const string PersistentPassword = "yourStrong(!)Password";

    private static MsSqlContainer? ephemeralContainer;
    private static string rootConnectionString = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        string persistentConnectionString = BuildConnectionString(PersistentHost, PersistentPort, PersistentPassword);

        if (await IsReachable(persistentConnectionString))
        {
            rootConnectionString = persistentConnectionString;
            return;
        }

        ephemeralContainer = new MsSqlBuilder(SqlServerImage)
            .WithTmpfsMount("/var/opt/mssql/data")
            .WithTmpfsMount("/var/opt/mssql/log")
            .WithTmpfsMount("/var/opt/mssql/secrets")
            .Build();

        await ephemeralContainer.StartAsync();
        rootConnectionString = ephemeralContainer.GetConnectionString();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (ephemeralContainer is not null)
        {
            await ephemeralContainer.DisposeAsync();
        }

        // Persistent container: per-test databases are left behind on
        // purpose (dropping them here or in each test's TearDown both cost
        // more than they're worth — see db-dev.sh's `clean` command).
    }

    public static string GetConnectionStringFor(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(rootConnectionString)
        {
            InitialCatalog = databaseName
        };

        return builder.ConnectionString;
    }

    private static string BuildConnectionString(string host, int port, string password)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"{host},{port}",
            UserID = "sa",
            Password = password,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private static async Task<bool> IsReachable(string connectionString)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await connection.OpenAsync(timeout.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
