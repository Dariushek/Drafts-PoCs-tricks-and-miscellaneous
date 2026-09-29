using System.Net;
using System.Net.Http.Json;
using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.WebApi.Books;

[Arguments(PersistenceKind.Ef)]
[Arguments(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind): WebApiFixture(persistenceKind)
{
    [Test]
    public async Task Book_with_copies_available_is_available_to_rent()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);
        Guid bookId = await GivenRegisteredBookIdAsync(command);

        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/availability");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BookAvailability>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.IsAvailable).IsTrue();
        await Assert.That(result.CopiesAvailable).IsEqualTo(3);
    }

    [Test]
    public async Task Book_with_no_copies_left_is_not_available_to_rent()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Guid bookId = await GivenRegisteredBookIdAsync(command);
        await GivenRentedBookAsync(bookId);

        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/availability");

        var result = await response.Content.ReadFromJsonAsync<BookAvailability>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.IsAvailable).IsFalse();
        await Assert.That(result.CopiesAvailable).IsEqualTo(0);
    }

    [Test]
    public async Task Checking_availability_of_unknown_book_returns_not_found()
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{Guid.NewGuid()}/availability");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}