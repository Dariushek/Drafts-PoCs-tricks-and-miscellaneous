using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;
using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DbMockInMemory.Tests.WebApi;

public abstract class WebApiFixture
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
                services.RemoveAll<DbContextOptions<BooksDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BooksDbContext>>();
                services.AddDbContext<BooksDbContext>(options => options.UseInMemoryDatabase(databaseName));
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
}