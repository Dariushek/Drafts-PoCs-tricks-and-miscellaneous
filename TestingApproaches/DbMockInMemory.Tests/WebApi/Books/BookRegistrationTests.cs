using System.Net;
using System.Net.Http.Json;
using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Mvc;

namespace DbMockInMemory.Tests.WebApi.Books;

[TestFixture(RepositoryKind.Ef)]
[TestFixture(RepositoryKind.Fake)]
public class BookRegistrationTests(RepositoryKind repositoryKind) : WebApiFixture(repositoryKind)
{
    [Test]
    public async Task Book_registration_registers_book()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var result = await response.Content.ReadFromJsonAsync<BookRegistrationResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.BookId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Book_with_negative_copies_available_cannot_be_registered()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", -1);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Errors.Keys, Has.Member("CopiesAvailable"));
    }

    [Test]
    public async Task Book_with_duplicate_isbn_cannot_be_registered()
    {
        var command = new BookRegistrationCommand("978-1-4919-5535-0", "Domain-Driven Design", "Eric Evans", 2);
        await GivenRegisteredBookAsync(command);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Status, Is.EqualTo((int)HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Book_cannot_be_registered_when_registration_is_closed()
    {
        await GivenRegistrationClosedAsync();
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}