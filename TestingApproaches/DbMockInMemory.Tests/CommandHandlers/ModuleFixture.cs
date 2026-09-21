using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbMockInMemory.Tests.CommandHandlers;

public abstract class ModuleFixture(PersistenceKind persistenceKind)
{
    private ServiceProvider provider = null!;

    protected IBookRegistrationHandler BookRegistrationHandler { get; private set; } = null!;
    protected IClosingRegistrationHandler ClosingRegistrationHandler { get; private set; } = null!;
    protected ICheckBookAvailabilityHandler CheckBookAvailabilityHandler { get; private set; } = null!;
    protected IBookRentingHandler BookRentingHandler { get; private set; } = null!;

    [SetUp]
    public void ModuleFixtureSetUp()
    {
        var services = new ServiceCollection();

        if (persistenceKind == PersistenceKind.Ef)
        {
            string databaseName = Guid.NewGuid().ToString();
            services.AddBusinessLogicModule(options => options.UseInMemoryDatabase(databaseName, InMemoryRoot.Instance), persistenceKind);
        }
        else
        {
            services.AddBusinessLogicModule(persistenceKind: persistenceKind);
        }

        provider = services.BuildServiceProvider();

        provider.InitializeBusinessLogicModuleDatabase();

        BookRegistrationHandler = provider.GetRequiredService<IBookRegistrationHandler>();
        ClosingRegistrationHandler = provider.GetRequiredService<IClosingRegistrationHandler>();
        CheckBookAvailabilityHandler = provider.GetRequiredService<ICheckBookAvailabilityHandler>();
        BookRentingHandler = provider.GetRequiredService<IBookRentingHandler>();
    }

    [TearDown]
    public void ModuleFixtureTearDown()
    {
        provider.Dispose();
    }

    protected Task<Result<BookRegistrationResult>> GivenRegisteredBook(BookRegistrationCommand command)
    {
        return BookRegistrationHandler.Handle(command, CancellationToken.None);
    }

    protected Task GivenRegistrationClosed()
    {
        return ClosingRegistrationHandler.Handle(new ClosingRegistrationCommand(), CancellationToken.None);
    }

    protected Task<Result<BookRentingResult>> GivenRentedBook(Guid bookId)
    {
        return BookRentingHandler.Handle(new BookRentingCommand(bookId), CancellationToken.None);
    }
}