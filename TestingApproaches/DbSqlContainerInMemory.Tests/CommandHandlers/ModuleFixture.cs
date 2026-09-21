using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbSqlContainerInMemory.Tests.CommandHandlers;

public abstract class ModuleFixture(PersistenceKind persistenceKind)
{
    private ServiceProvider provider = null!;

    protected IRegisterBookHandler RegisterBookHandler { get; private set; } = null!;
    protected ICloseRegistrationHandler CloseRegistrationHandler { get; private set; } = null!;
    protected ICheckBookAvailabilityHandler CheckBookAvailabilityHandler { get; private set; } = null!;
    protected IRentBookHandler RentBookHandler { get; private set; } = null!;

    [SetUp]
    public void ModuleFixtureSetUp()
    {
        var databaseName = $"test_{Guid.NewGuid():N}";
        string connectionString = SqlContainerSetup.GetConnectionStringFor(databaseName);

        var services = new ServiceCollection();
        services.AddBusinessLogicModule(options => options.UseSqlServer(connectionString), persistenceKind);

        provider = services.BuildServiceProvider();

        provider.InitializeBusinessLogicModuleDatabase();

        RegisterBookHandler = provider.GetRequiredService<IRegisterBookHandler>();
        CloseRegistrationHandler = provider.GetRequiredService<ICloseRegistrationHandler>();
        CheckBookAvailabilityHandler = provider.GetRequiredService<ICheckBookAvailabilityHandler>();
        RentBookHandler = provider.GetRequiredService<IRentBookHandler>();
    }

    [TearDown]
    public void ModuleFixtureTearDown() { provider.Dispose(); }

    protected Task<Result<BookRegistration>> GivenRegisteredBook
        (RegisterBook command) =>
        RegisterBookHandler.Handle(command, CancellationToken.None);

    protected Task GivenRegistrationClosed() => CloseRegistrationHandler.Handle(new(), CancellationToken.None);

    protected Task<Result<BookRenting>> GivenRentedBook
        (Guid bookId) =>
        RentBookHandler.Handle(new(bookId), CancellationToken.None);
}