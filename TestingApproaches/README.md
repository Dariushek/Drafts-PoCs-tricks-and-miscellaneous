# TestingApproaches

Compares ways of testing against a database: in-memory EF Core provider (`DbMockInMemory.Tests`), a real SQL Server via Testcontainers (`DbSqlContainerInMemory.Tests`), and a real
MongoDB via Testcontainers (`DbMongoContainerInMemory.Tests`).

The two SQL-backed projects are further split by persistence implementation,
via `BusinessLogicModule`'s `IBookRepository` abstraction (see
`BusinessLogicModule/Persistence/`) and a single, parameterized
`AddBusinessLogicModule(configureDbContext, persistenceKind: PersistenceKind.Ef)`
extension method (`PersistenceKind` defaults to `Ef`, so `Program.cs`'s
production call site is untouched):

| Project                          | `PersistenceKind.Ef` | second variant                                                                                                                                       |
|----------------------------------|----------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| `DbMockInMemory.Tests`           | EF InMemory provider | `PersistenceKind.Fake` — hand-rolled in-memory dictionary, no EF at all                                                                              |
| `DbSqlContainerInMemory.Tests`   | EF Core + SQL Server | `PersistenceKind.PlainSql` — raw ADO.NET (`Microsoft.Data.SqlClient`) against the same SQL Server                                                    |
| `DbMongoContainerInMemory.Tests` | *(n/a)*              | `PersistenceKind.Mongo` only — `MongoDB.Driver` is already the "no ORM" way of talking to Mongo, so there's no second variant to contrast it against |

Every test class in the two-variant projects carries both
`[TestFixture(PersistenceKind...)]` attributes and runs its bodies against
each — same assertions, different persistence code underneath.
`DbMongoContainerInMemory.Tests` has only one variant, so its test classes
skip the `[TestFixture(...)]` parameterization entirely.

## Run tests

```bash
dotnet test DbMockInMemory.Tests/DbMockInMemory.Tests.csproj
dotnet test DbSqlContainerInMemory.Tests/DbSqlContainerInMemory.Tests.csproj
dotnet test DbMongoContainerInMemory.Tests/DbMongoContainerInMemory.Tests.csproj
```

`DbSqlContainerInMemory.Tests` and `DbMongoContainerInMemory.Tests` each
start their own container per run unless their persistent dev container (below) is running.

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

## Speed up Mongo container tests: persistent dev container

Same idea, `db-dev-mongo.sh` this time — a long-lived MongoDB container on
its own fixed port (14340, so it can run alongside the SQL one on 14330).
`MongoContainerSetup` connects to it directly when reachable, falling back
to an ephemeral Testcontainers-raised one otherwise.

```bash
./db-dev-mongo.sh up       # start once
dotnet test DbMongoContainerInMemory.Tests/DbMongoContainerInMemory.Tests.csproj
./db-dev-mongo.sh down     # stop when done (or leave it running)
./db-dev-mongo.sh status   # check state
./db-dev-mongo.sh clean    # reset: wipe accumulated test databases
```

## EF InMemory gotcha (`DbMockInMemory.Tests`)

`UseInMemoryDatabase(name)` alone doesn't reliably share a named database
across different scopes (e.g. a `WebApplicationFactory` request vs. the
scope that ran `EnsureCreated`) — rows and even `HasData` seeds can appear
missing. Fix: pass an explicit, shared `InMemoryDatabaseRoot`
(`DbMockInMemory.Tests/InMemoryRoot.cs`) to every `UseInMemoryDatabase` call,
and compute the database name as a variable *before* the options lambda, not
inline inside it (the lambda can run more than once; inline
`Guid.NewGuid()` silently produces a different database per invocation).

## MongoDB.Driver gotcha (`DbMongoContainerInMemory.Tests`)

`MongoDB.Driver` 3.x throws `BsonSerializationException` serializing a bare
`Guid` property ("GuidSerializer cannot serialize a Guid when
GuidRepresentation is Unspecified") — the implicit `GuidRepresentation` EF
and older driver versions had is gone. Fix: `[BsonRepresentation(BsonType.String)]`
on every `Guid` property (see `BookDocument.cs`) — stores it as a plain
string, sidestepping the byte-order ambiguity `GuidRepresentation` variants
exist to solve in the first place.

## How tests are isolated

Isolation is per-database, not per-container, for all three container-backed
projects. `DbSqlContainerInMemory.Tests` and `DbMongoContainerInMemory.Tests`
each share one server instance (ephemeral or the persistent dev container),
but every test gets its own database on it:

- Every test's `[SetUp]` generates a unique database name (`test_{Guid.NewGuid():N}`, see `ModuleFixture.cs` / `WebApiFixture.cs`).
- The connection string points at that database on the shared server.
- `Database.EnsureCreated()` (SQL) builds that database's schema fresh
  before the test runs; Mongo has no schema to create, but
  `InitializeBusinessLogicModuleDatabase()` still upserts the registration
  window singleton document there — Mongo has no `HasData`-style seeding.
- This is what makes parallel test execution safe: no two tests ever touch
  the same database.

`[TearDown]` disposes the DB context / `WebApplicationFactory` but does **not** drop the database — dropping per test or in bulk both turned out
slower/flakier than just leaving it (a stray database is harmless; a 30s
`ALTER DATABASE` timeout mid-run isn't, and the same logic applies to
Mongo). Databases accumulate on the persistent containers across runs as a
result — run `./db-dev.sh clean` / `./db-dev-mongo.sh clean` to reset them.
The ephemeral containers need no cleanup since the whole container is
thrown away after each run.

## Benchmark

```bash
./bench.sh              # run 5x per project, print stats
./bench.sh run 10       # just run, 10x
./bench.sh report       # just report on existing bench-results/
```

### Per-test speed: it's not "in-memory vs. networked", it's ceremony

Measured per-test mean, `DbMockInMemory.Tests`'s two variants vs.
`DbMongoContainerInMemory.Tests` (single-threaded, from each project's trx):

```
Mock/Ef:   mean=27.7ms/test
Mock/Fake: mean= 4.5ms/test
Mongo:     mean=20.7ms/test
```

A real database, over a real TCP connection to a container, beats an
in-process "fake" EF provider that does zero I/O. The intuitive "in-memory
must be fastest" story doesn't hold once you look at what each path
actually does per call:

- `EfBookRepository` against `UseInMemoryDatabase` still pays EF's full
  ceremony on every operation - LINQ-to-InMemory query translation,
  change-tracker snapshotting, reflection-backed entity materialization -
  even though there's no disk or network underneath it.
- `MongoBookRepository` is a direct wire-protocol call straight from domain
  object to BSON (`Find` by `_id`, `InsertOne`, `ReplaceOne`) - no provider
  abstraction layer in between. The network hop to a local container costs
  less than EF's abstraction does.
- `InMemoryFakeBookRepository` has neither cost: no translation layer, no
  I/O, just a dictionary - hence the ~4-6x gap over both.

So the cost driver isn't "in-memory vs. networked" - it's how much
machinery sits between your code and the actual read/write.
