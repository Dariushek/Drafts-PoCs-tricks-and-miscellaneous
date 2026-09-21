using System.Net;
using System.Net.Http.Json;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.WebApi.Books;

[TestFixture(RepositoryKind.Ef)]
[TestFixture(RepositoryKind.Fake)]
public class BookRentingTests(RepositoryKind repositoryKind) : WebApiFixture(repositoryKind)
{
    [Test]
    public async Task Renting_a_book_decreases_copies_available()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 2);
        Guid bookId = await GivenRegisteredBookIdAsync(command);

        HttpResponseMessage response = await Client.PostAsync($"/books/{bookId}/rent", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<BookRentingResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CopiesAvailable, Is.EqualTo(1));
    }

    [Test]
    public async Task Book_out_of_copies_cannot_be_rented()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Guid bookId = await GivenRegisteredBookIdAsync(command);
        await GivenRentedBookAsync(bookId);

        HttpResponseMessage response = await Client.PostAsync($"/books/{bookId}/rent", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Unknown_book_cannot_be_rented()
    {
        HttpResponseMessage response = await Client.PostAsync($"/books/{Guid.NewGuid()}/rent", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
