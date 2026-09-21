using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbSqlContainerInMemory.Tests.WebApi;

public abstract class WebApiFixture(PersistenceKind persistenceKind)
{
    private WebApplicationFactory<Program> Factory { get; set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void WebApiFixtureSetUp()
    {
        string databaseName = $"test_{Guid.NewGuid():N}";
        string connectionString = SqlContainerSetup.GetConnectionStringFor(databaseName);

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveBusinessLogicModule();
                services.AddBusinessLogicModule(options => options.UseSqlServer(connectionString), persistenceKind);
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
