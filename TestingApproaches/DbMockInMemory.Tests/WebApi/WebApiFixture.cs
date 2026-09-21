using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbMockInMemory.Tests.WebApi;

public abstract class WebApiFixture(PersistenceKind persistenceKind)
{
    private WebApplicationFactory<Program> Factory { get; set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void WebApiFixtureSetUp()
    {
        string databaseName = Guid.NewGuid().ToString();

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveBusinessLogicModule();

                if (persistenceKind == PersistenceKind.Ef)
                {
                    services.AddBusinessLogicModule(options => options.UseInMemoryDatabase(databaseName, InMemoryRoot.Instance), persistenceKind);
                }
                else
                {
                    services.AddBusinessLogicModule(persistenceKind: persistenceKind);
                }
            })
        );

        Factory.Services.InitializeBusinessLogicModuleDatabase();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void WebApiFixtureTearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected Task<HttpResponseMessage> GivenRegisteredBookAsync(BookRegistrationCommand command)
    {
        return Client.PostAsJsonAsync("/books", command);
    }

    protected Task<HttpResponseMessage> GivenRegistrationClosedAsync()
    {
        return Client.PostAsync("/books/registration/close", null);
    }

    protected async Task<Guid> GivenRegisteredBookIdAsync(BookRegistrationCommand command)
    {
        HttpResponseMessage response = await GivenRegisteredBookAsync(command);
        var result = await response.Content.ReadFromJsonAsync<BookRegistrationResult>();

        return result!.BookId;
    }

    protected Task<HttpResponseMessage> GivenRentedBookAsync(Guid bookId)
    {
        return Client.PostAsync($"/books/{bookId}/rent", null);
    }
}