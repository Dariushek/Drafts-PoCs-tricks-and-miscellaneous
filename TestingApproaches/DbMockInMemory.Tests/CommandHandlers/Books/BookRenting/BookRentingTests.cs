using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.CommandHandlers.Books.BookRenting;

[TestFixture(PersistenceKind.Ef)]
[TestFixture(PersistenceKind.Fake)]
public class BookRentingTests(PersistenceKind persistenceKind) : ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Renting_a_book_decreases_copies_available()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 2);
        Result<BookRegistrationResult> registered = await GivenRegisteredBook(command);

        Result<BookRentingResult> result = await BookRentingHandler.Handle(
            new BookRentingCommand(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.CopiesAvailable, Is.EqualTo(1));
    }

    [Test]
    public async Task Book_out_of_copies_cannot_be_rented()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Result<BookRegistrationResult> registered = await GivenRegisteredBook(command);
        await GivenRentedBook(registered.Value.BookId);

        Result<BookRentingResult> result = await BookRentingHandler.Handle(
            new BookRentingCommand(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 409));
    }

    [Test]
    public async Task Unknown_book_cannot_be_rented()
    {
        Result<BookRentingResult> result = await BookRentingHandler.Handle(
            new BookRentingCommand(Guid.NewGuid()), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 404));
    }
}
