using System.Net.Http.Json;
using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DbMockInMemory.Tests.WebApi;

public abstract class WebApiFixture
{
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void WebApiFixtureSetUp()
    {
        Factory = new WebApplicationFactory<Program>();
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