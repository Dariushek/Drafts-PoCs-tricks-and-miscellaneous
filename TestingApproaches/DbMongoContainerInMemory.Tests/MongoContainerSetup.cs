using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace DbMongoContainerInMemory.Tests;

[SetUpFixture]
public class MongoContainerSetup
{
    private const string MongoImage = "mongo:6.0";

    // Matches db-dev-mongo.sh. A long-lived container started once for the
    // local dev loop; when it's reachable we connect straight to it instead
    // of paying the Mongo boot cost on every `dotnet test` run. CI has no
    // such container running, so it falls through to the ephemeral one
    // below. See SqlContainerSetup for the SQL Server equivalent.
    private const string PersistentHost = "localhost";
    private const int PersistentPort = 14340;
    private const string PersistentUsername = "root";
    private const string PersistentPassword = "yourStrongPassword";

    private static MongoDbContainer? ephemeralContainer;
    private static string rootConnectionString = null!;

    public static string ConnectionString => rootConnectionString;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        string persistentConnectionString = BuildConnectionString(PersistentHost, PersistentPort, PersistentUsername, PersistentPassword);

        if (await IsReachable(persistentConnectionString))
        {
            rootConnectionString = persistentConnectionString;
            return;
        }

        ephemeralContainer = new MongoDbBuilder(MongoImage)
            .WithUsername(PersistentUsername)
            .WithPassword(PersistentPassword)
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
        // purpose - same reasoning as SqlContainerSetup. Run
        // ./db-dev-mongo.sh clean to reset it.
    }

    private static string BuildConnectionString(string host, int port, string username, string password) =>
        $"mongodb://{username}:{password}@{host}:{port}/?directConnection=true";

    private static async Task<bool> IsReachable(string connectionString)
    {
        try
        {
            var client = new MongoClient(connectionString);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await client.ListDatabaseNamesAsync(timeout.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
