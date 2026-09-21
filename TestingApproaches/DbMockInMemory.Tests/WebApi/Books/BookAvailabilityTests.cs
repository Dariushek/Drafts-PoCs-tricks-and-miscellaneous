using System.Net;
using System.Net.Http.Json;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.WebApi.Books;

[TestFixture(RepositoryKind.Ef)]
[TestFixture(RepositoryKind.Fake)]
public class BookAvailabilityTests(RepositoryKind repositoryKind) : WebApiFixture(repositoryKind)
{
    [Test]
    public async Task Book_with_copies_available_is_available_to_rent()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);
        Guid bookId = await GivenRegisteredBookIdAsync(command);

        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/availability");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<BookAvailabilityResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsAvailable, Is.True);
        Assert.That(result.CopiesAvailable, Is.EqualTo(3));
    }

    [Test]
    public async Task Book_with_no_copies_left_is_not_available_to_rent()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Guid bookId = await GivenRegisteredBookIdAsync(command);
        await GivenRentedBookAsync(bookId);

        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/availability");

        var result = await response.Content.ReadFromJsonAsync<BookAvailabilityResult>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsAvailable, Is.False);
        Assert.That(result.CopiesAvailable, Is.EqualTo(0));
    }

    [Test]
    public async Task Checking_availability_of_unknown_book_returns_not_found()
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{Guid.NewGuid()}/availability");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
