using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.BookRenting;

[TestFixture(PersistenceKind.Ef)]
[TestFixture(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind) : ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Renting_a_book_decreases_copies_available()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 2);
        Result<BusinessLogicModule.Books.BookRegistration> registered = await GivenRegisteredBook(command);

        Result<BusinessLogicModule.Books.BookRenting> result = await RentBookHandler.Handle(
            new RentBook(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.CopiesAvailable, Is.EqualTo(1));
    }

    [Test]
    public async Task Book_out_of_copies_cannot_be_rented()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Result<BusinessLogicModule.Books.BookRegistration> registered = await GivenRegisteredBook(command);
        await GivenRentedBook(registered.Value.BookId);

        Result<BusinessLogicModule.Books.BookRenting> result = await RentBookHandler.Handle(
            new RentBook(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 409));
    }

    [Test]
    public async Task Unknown_book_cannot_be_rented()
    {
        Result<BusinessLogicModule.Books.BookRenting> result = await RentBookHandler.Handle(
            new RentBook(Guid.NewGuid()), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 404));
    }
}
