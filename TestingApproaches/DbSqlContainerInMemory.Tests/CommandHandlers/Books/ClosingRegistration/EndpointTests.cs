using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.ClosingRegistration;

[Arguments(PersistenceKind.Ef)]
[Arguments(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind): ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Closing_registration_prevents_new_books_from_being_registered()
    {
        await CloseRegistrationHandler.Handle(new(), CancellationToken.None);
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle
            (command, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Errors).Contains(error => error.StatusCode == 400);
    }
}