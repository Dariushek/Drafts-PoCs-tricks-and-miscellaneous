using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.BookRegistration;

[TestFixture(PersistenceKind.Ef)]
[TestFixture(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind) : ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Book_registration_registers_book()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.BookId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Book_with_negative_copies_available_cannot_be_registered()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", -1);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error is { Field: "CopiesAvailable" }));
    }

    [Test]
    public async Task Book_with_duplicate_isbn_cannot_be_registered()
    {
        var command = new RegisterBook("978-1-4919-5535-0", "Domain-Driven Design", "Eric Evans", 2);
        await GivenRegisteredBook(command);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 409));
    }

    [Test]
    public async Task Book_cannot_be_registered_when_registration_is_closed()
    {
        await GivenRegistrationClosed();
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error is { Field: null, StatusCode: 400 }));
    }
}
