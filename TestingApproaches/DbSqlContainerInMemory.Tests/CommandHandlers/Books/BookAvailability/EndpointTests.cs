using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.BookAvailability;

[TestFixture(PersistenceKind.Ef)]
[TestFixture(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind) : ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Book_with_copies_available_is_available_to_rent()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);
        Result<BusinessLogicModule.Books.BookRegistration> registered = await GivenRegisteredBook(command);

        Result<BusinessLogicModule.Books.BookAvailability> result = await CheckBookAvailabilityHandler.Handle(
            new BusinessLogicModule.Books.CheckBookAvailability(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsAvailable, Is.True);
        Assert.That(result.Value.CopiesAvailable, Is.EqualTo(3));
    }

    [Test]
    public async Task Book_with_no_copies_left_is_not_available_to_rent()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 1);
        Result<BusinessLogicModule.Books.BookRegistration> registered = await GivenRegisteredBook(command);
        await GivenRentedBook(registered.Value.BookId);

        Result<BusinessLogicModule.Books.BookAvailability> result = await CheckBookAvailabilityHandler.Handle(
            new BusinessLogicModule.Books.CheckBookAvailability(registered.Value.BookId), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.IsAvailable, Is.False);
        Assert.That(result.Value.CopiesAvailable, Is.EqualTo(0));
    }

    [Test]
    public async Task Checking_availability_of_unknown_book_fails()
    {
        Result<BusinessLogicModule.Books.BookAvailability> result = await CheckBookAvailabilityHandler.Handle(
            new BusinessLogicModule.Books.CheckBookAvailability(Guid.NewGuid()), CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 404));
    }
}
