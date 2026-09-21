# TestingApproaches

Compares two ways of testing against a database: in-memory EF Core provider
(`DbMockInMemory.Tests`) vs. a real SQL Server via Testcontainers
(`DbSqlContainerInMemory.Tests`).

## Run tests

```bash
dotnet test DbMockInMemory.Tests/DbMockInMemory.Tests.csproj
dotnet test DbSqlContainerInMemory.Tests/DbSqlContainerInMemory.Tests.csproj
```

`DbSqlContainerInMemory.Tests` starts its own SQL Server container per run
(~6s) unless the persistent dev container below is running.

## Speed up SQL container tests: persistent dev container

`db-dev.sh` starts a long-lived SQL Server container on a fixed port. When
it's up, `SqlContainerSetup` connects to it directly instead of starting a
fresh container each run (~1.7s instead of ~6s).

```bash
./db-dev.sh up       # start once
dotnet test DbSqlContainerInMemory.Tests/DbSqlContainerInMemory.Tests.csproj
dotnet test DbSqlContainerInMemory.Tests/DbSqlContainerInMemory.Tests.csproj  # fast again
./db-dev.sh down     # stop when done (or leave it running)
./db-dev.sh status   # check state
./db-dev.sh clean    # reset: wipe accumulated test databases (see below)
```

If it's not running (CI, fresh machine), tests fall back to the ephemeral
per-run container automatically — no config needed either way.

Container uses tmpfs for data/log/secrets, so `up` always recreates it from
scratch rather than restarting a stopped one (a stopped container with tmpfs
mounts fails to reinitialize as the non-root `mssql` user).

## How tests are isolated

Isolation is per-database, not per-container. All `DbSqlContainerInMemory.Tests`
share one SQL Server instance (ephemeral or the persistent dev container),
but each test gets its own database on it:

- Every test's `[SetUp]` generates a unique database name
  (`test_{Guid.NewGuid():N}`, see `ModuleFixture.cs` / `WebApiFixture.cs`).
- The connection string points at that database on the shared server.
- `Database.EnsureCreated()` builds that database's schema fresh before the
  test runs — no migrations, no shared seed data.
- This is what makes parallel test execution safe: no two tests ever touch
  the same database.

`[TearDown]` disposes the DB context / `WebApplicationFactory` but does
**not** drop the database — dropping per test or in bulk both turned out
slower/flakier than just leaving it (a stray database is harmless; a 30s
`ALTER DATABASE` timeout mid-run isn't). Databases accumulate on the
persistent container across runs as a result — run `./db-dev.sh clean` to
reset it. The ephemeral container needs no cleanup since the whole
container is thrown away after each run.

## Benchmark

```bash
./bench.sh              # run 5x per project, print stats
./bench.sh run 10       # just run, 10x
./bench.sh report       # just report on existing bench-results/
```
