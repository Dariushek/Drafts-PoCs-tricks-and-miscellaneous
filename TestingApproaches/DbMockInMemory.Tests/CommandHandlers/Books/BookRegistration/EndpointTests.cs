using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.CommandHandlers.Books.BookRegistration;

[Arguments(PersistenceKind.Ef)]
[Arguments(PersistenceKind.Fake)]
public class EndpointTests(PersistenceKind persistenceKind): ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Book_registration_registers_book()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle
            (command, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.BookId).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task Book_with_negative_copies_available_cannot_be_registered()
    {
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", -1);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle
            (command, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Errors).Contains(error => error is { Field: "CopiesAvailable" });
    }

    [Test]
    public async Task Book_with_duplicate_isbn_cannot_be_registered()
    {
        var command = new RegisterBook("978-1-4919-5535-0", "Domain-Driven Design", "Eric Evans", 2);
        await GivenRegisteredBook(command);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle
            (command, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Errors).Contains(error => error.StatusCode == 409);
    }

    [Test]
    public async Task Book_cannot_be_registered_when_registration_is_closed()
    {
        await GivenRegistrationClosed();
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle
            (command, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Errors).Contains(error => error is { Field: null, StatusCode: 400 });
    }
}