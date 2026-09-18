using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbMockInMemory.Tests.CommandHandlers;

public abstract class ModuleFixture
{
    private ServiceProvider provider = null!;

    protected IBookRegistrationHandler BookRegistrationHandler { get; private set; } = null!;
    protected IClosingRegistrationHandler ClosingRegistrationHandler { get; private set; } = null!;
    protected IBookAvailabilityHandler BookAvailabilityHandler { get; private set; } = null!;
    protected IBookRentingHandler BookRentingHandler { get; private set; } = null!;

    [SetUp]
    public void ModuleFixtureSetUp()
    {
        string databaseName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddBusinessLogicModule(options => options.UseInMemoryDatabase(databaseName));
        provider = services.BuildServiceProvider();

        provider.InitializeBusinessLogicModuleDatabase();

        BookRegistrationHandler = provider.GetRequiredService<IBookRegistrationHandler>();
        ClosingRegistrationHandler = provider.GetRequiredService<IClosingRegistrationHandler>();
        BookAvailabilityHandler = provider.GetRequiredService<IBookAvailabilityHandler>();
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