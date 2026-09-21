using System.Net;
using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.WebApi.Books;

[TestFixture(PersistenceKind.Ef)]
[TestFixture(PersistenceKind.PlainSql)]
public class ClosingRegistrationTests(PersistenceKind persistenceKind): WebApiFixture(persistenceKind)
{
    [Test]
    public async Task Closing_registration_returns_no_content()
    {
        HttpResponseMessage response = await Client.PostAsync("/books/registration/close", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Closing_registration_prevents_new_books_from_being_registered()
    {
        await GivenRegistrationClosedAsync();
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}