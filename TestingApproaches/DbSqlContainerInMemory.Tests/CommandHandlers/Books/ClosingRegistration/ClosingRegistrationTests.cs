using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbSqlContainerInMemory.Tests.CommandHandlers.Books.ClosingRegistration;

public class ClosingRegistrationTests : ModuleFixture
{
    [Test]
    public async Task Closing_registration_prevents_new_books_from_being_registered()
    {
        await ClosingRegistrationHandler.Handle(new ClosingRegistrationCommand(), CancellationToken.None);
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BookRegistrationResult> result = await BookRegistrationHandler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.StatusCode == 400));
    }
}
