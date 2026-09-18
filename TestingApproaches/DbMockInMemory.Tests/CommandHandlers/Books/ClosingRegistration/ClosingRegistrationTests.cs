using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.CommandHandlers.Books.ClosingRegistration;

public class ClosingRegistrationTests : ModuleFixture
{
    [Test]
    public void Closing_registration_prevents_new_books_from_being_registered()
    {
        ClosingRegistrationHandler.Handle(new ClosingRegistrationCommand());
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BookRegistrationResult> result = BookRegistrationHandler.Handle(command);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.Type == ErrorType.Domain && error.StatusCode == 400));
    }
}