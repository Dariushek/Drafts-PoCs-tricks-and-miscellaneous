using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.BookAvailability;

[Arguments(PersistenceKind.Ef)]
[Arguments(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind)
    : ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Book_with_copies_available_is_available_to_rent()
    {
        var command = new RegisterBook(
            "978-0-13-468599-1",
            "Clean Code",
            "Robert C. Martin",
            3);
        var registered = await GivenRegisteredBook(command);

        var result = await CheckBookAvailabilityHandler.Handle(
            new(registered.Value.BookId),
            CancellationToken.None);

        await Assert.That(result.IsSuccess)
            .IsTrue();
        await Assert.That(result.Value.IsAvailable)
            .IsTrue();
        await Assert.That(result.Value.CopiesAvailable)
            .IsEqualTo(3);
    }

    [Test]
    public async Task Book_with_no_copies_left_is_not_available_to_rent()
    {
        var command = new RegisterBook(
            "978-0-13-468599-1",
            "Clean Code",
            "Robert C. Martin",
            1);
        var registered = await GivenRegisteredBook(command);
        await GivenRentedBook(registered.Value.BookId);

        var result = await CheckBookAvailabilityHandler.Handle(
            new(registered.Value.BookId),
            CancellationToken.None);

        await Assert.That(result.IsSuccess)
            .IsTrue();
        await Assert.That(result.Value.IsAvailable)
            .IsFalse();
        await Assert.That(result.Value.CopiesAvailable)
            .IsEqualTo(0);
    }

    [Test]
    public async Task Checking_availability_of_unknown_book_fails()
    {
        var result = await CheckBookAvailabilityHandler.Handle(
            new(Guid.NewGuid()),
            CancellationToken.None);

        await Assert.That(result.IsSuccess)
            .IsFalse();
        await Assert.That(result.Errors)
            .Contains(error => error.StatusCode == 404);
    }
}