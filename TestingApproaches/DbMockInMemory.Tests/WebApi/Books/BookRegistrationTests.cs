using System.Net;
using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Mvc;

namespace DbMockInMemory.Tests.WebApi.Books;

[Arguments(PersistenceKind.Ef)]
[Arguments(PersistenceKind.Fake)]
public class BookRegistrationTests(PersistenceKind persistenceKind): WebApiFixture(persistenceKind)
{
    [Test]
    public async Task Book_registration_registers_book()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<BookRegistration>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.BookId).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task Book_with_negative_copies_available_cannot_be_registered()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", -1);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        await Assert.That(problem).IsNotNull();
        await Assert.That(problem!.Errors.Keys).Contains("CopiesAvailable");
    }

    [Test]
    public async Task Book_with_duplicate_isbn_cannot_be_registered()
    {
        var command = new RegisterBook("978-1-4919-5535-0", "Domain-Driven Design", "Eric Evans", 2);
        await GivenRegisteredBookAsync(command);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        await Assert.That(problem).IsNotNull();
        await Assert.That(problem!.Status).IsEqualTo((int)HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Book_cannot_be_registered_when_registration_is_closed()
    {
        await GivenRegistrationClosedAsync();
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        HttpResponseMessage response = await Client.PostAsJsonAsync("/books", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}