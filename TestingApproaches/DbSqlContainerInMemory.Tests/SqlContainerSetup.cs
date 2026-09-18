using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace DbSqlContainerInMemory.Tests;

[SetUpFixture]
public class SqlContainerSetup
{
    private static MsSqlContainer container = null!;

    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        container = new MsSqlBuilder(SqlServerImage)
            .WithTmpfsMount("/var/opt/mssql/data")
            .WithTmpfsMount("/var/opt/mssql/log")
            .WithTmpfsMount("/var/opt/mssql/secrets")
            .Build();

        await container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await container.DisposeAsync();
    }

    public static string GetConnectionStringFor(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = databaseName
        };

        return builder.ConnectionString;
    }
}
