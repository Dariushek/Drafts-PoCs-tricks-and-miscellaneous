using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.Extensions.DependencyInjection;

namespace DbMockInMemory.Tests.CommandHandlers;

public abstract class ModuleFixture
{
    private ServiceProvider provider = null!;

    protected IBookRegistrationHandler BookRegistrationHandler { get; private set; } = null!;
    protected IClosingRegistrationHandler ClosingRegistrationHandler { get; private set; } = null!;

    [SetUp]
    public void ModuleFixtureSetUp()
    {
        var services = new ServiceCollection();
        services.AddBusinessLogicModule();
        provider = services.BuildServiceProvider();

        BookRegistrationHandler = provider.GetRequiredService<IBookRegistrationHandler>();
        ClosingRegistrationHandler = provider.GetRequiredService<IClosingRegistrationHandler>();
    }

    [TearDown]
    public void ModuleFixtureTearDown()
    {
        provider.Dispose();
    }

    protected Result<BookRegistrationResult> GivenRegisteredBook(BookRegistrationCommand command)
    {
        return BookRegistrationHandler.Handle(command);
    }

    protected void GivenRegistrationClosed()
    {
        ClosingRegistrationHandler.Handle(new ClosingRegistrationCommand());
    }
}