using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DbMongoContainerInMemory.Tests.WebApi;

public abstract class WebApiFixture
{
    private WebApplicationFactory<Program> Factory { get; set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void WebApiFixtureSetUp()
    {
        var databaseName = $"test_{Guid.NewGuid():N}";

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                                                                              builder.ConfigureServices(services =>
                                                                                  {
                                                                                      services
                                                                                          .RemoveBusinessLogicModule();
                                                                                      services.AddBusinessLogicModule(
                                                                                          persistenceKind:
                                                                                          PersistenceKind.Mongo,
                                                                                          mongoConnectionString:
                                                                                          MongoContainerSetup
                                                                                              .ConnectionString,
                                                                                          mongoDatabaseName:
                                                                                          databaseName
                                                                                      );
                                                                                  }
                                                                              )
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

    protected Task<HttpResponseMessage> GivenRegisteredBookAsync
        (RegisterBook command) =>
        Client.PostAsJsonAsync("/books", command);

    protected Task<HttpResponseMessage> GivenRegistrationClosedAsync() =>
        Client.PostAsync("/books/registration/close", null);

    protected async Task<Guid> GivenRegisteredBookIdAsync(RegisterBook command)
    {
        HttpResponseMessage response = await GivenRegisteredBookAsync(command);
        var result = await response.Content.ReadFromJsonAsync<BookRegistration>();

        return result!.BookId;
    }

    protected Task<HttpResponseMessage> GivenRentedBookAsync
        (Guid bookId) =>
        Client.PostAsync($"/books/{bookId}/rent", null);
}