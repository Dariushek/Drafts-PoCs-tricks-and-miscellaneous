using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.CommandHandlers.Books.BookAvailability;

[TestFixture(RepositoryKind.Ef)]
[TestFixture(RepositoryKind.Fake)]
public class BookAvailabilityTests(RepositoryKind repositoryKind) : ModuleFixture(repositoryKind)
{
    [Test]
    public async Task Book_with_copies_available_is_available_to_rent()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);
        Result<BookRegistrationResult> registered = await GivenRegisteredBook(command);

        Result<BookAvailabilityResult> result = await BookAvailabilityHandler.Handle(
            new BookAvailabilityQuery(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsAvailable, Is.True);
        Assert.That(result.Value.CopiesAvailable, Is.EqualTo(3));
    }

    [Test]
    public async Task Book_with_no_copies_left_is_not_available_to_rent()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Result<BookRegistrationResult> registered = await GivenRegisteredBook(command);
        await GivenRentedBook(registered.Value.BookId);

        Result<BookAvailabilityResult> result = await BookAvailabilityHandler.Handle(
            new BookAvailabilityQuery(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsAvailable, Is.False);
        Assert.That(result.Value.CopiesAvailable, Is.EqualTo(0));
    }

    [Test]
    public async Task Checking_availability_of_unknown_book_fails()
    {
        Result<BookAvailabilityResult> result = await BookAvailabilityHandler.Handle(
            new BookAvailabilityQuery(Guid.NewGuid()), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 404));
    }
}
