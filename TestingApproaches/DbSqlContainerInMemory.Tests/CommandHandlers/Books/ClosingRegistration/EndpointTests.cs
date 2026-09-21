using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.ClosingRegistration;

[TestFixture(PersistenceKind.Ef)]
[TestFixture(PersistenceKind.PlainSql)]
public class EndpointTests(PersistenceKind persistenceKind) : ModuleFixture(persistenceKind)
{
    [Test]
    public async Task Closing_registration_prevents_new_books_from_being_registered()
    {
        await CloseRegistrationHandler.Handle(new CloseRegistration(), CancellationToken.None);
        var command = new RegisterBook("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BusinessLogicModule.Books.BookRegistration> result = await RegisterBookHandler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 400));
    }
}
