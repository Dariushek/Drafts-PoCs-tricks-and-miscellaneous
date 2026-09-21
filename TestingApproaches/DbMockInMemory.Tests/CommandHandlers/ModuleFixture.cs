using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbMockInMemory.Tests.CommandHandlers;

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
        var services = new ServiceCollection();

        if (persistenceKind == PersistenceKind.Ef)
        {
            var databaseName = Guid.NewGuid()
                                   .ToString();
            services.AddBusinessLogicModule
                (options => options.UseInMemoryDatabase(databaseName, InMemoryRoot.Instance), persistenceKind);
        }
        else
            services.AddBusinessLogicModule(persistenceKind: persistenceKind);

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
        (RegisterBook command)
        => RegisterBookHandler.Handle(command, CancellationToken.None);

    protected Task GivenRegistrationClosed() => CloseRegistrationHandler.Handle(new(), CancellationToken.None);

    protected Task<Result<BookRenting>> GivenRentedBook
        (Guid bookId)
        => RentBookHandler.Handle(new(bookId), CancellationToken.None);
}