using BusinessLogicModule;
using BusinessLogicModule.Books;

namespace DbMockInMemory.Tests.CommandHandlers.Books.BookRegistration;

public class BookRegistrationTests : ModuleFixture
{
    [Test]
    public void Book_registration_registers_book()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BookRegistrationResult> result = BookRegistrationHandler.Handle(command);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.BookId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Book_with_negative_copies_available_cannot_be_registered()
    {
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", -1);

        Result<BookRegistrationResult> result = BookRegistrationHandler.Handle(command);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.Type == ErrorType.Validation && error.Field == "CopiesAvailable"));
    }

    [Test]
    public void Book_with_duplicate_isbn_cannot_be_registered()
    {
        var command = new BookRegistrationCommand("978-1-4919-5535-0", "Domain-Driven Design", "Eric Evans", 2);
        GivenRegisteredBook(command);

        Result<BookRegistrationResult> result = BookRegistrationHandler.Handle(command);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.Type == ErrorType.Domain && error.StatusCode == 409));
    }

    [Test]
    public void Book_cannot_be_registered_when_registration_is_closed()
    {
        GivenRegistrationClosed();
        var command = new BookRegistrationCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        Result<BookRegistrationResult> result = BookRegistrationHandler.Handle(command);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<Error>(error => error.Type == ErrorType.Domain && error.Field == null && error.StatusCode == 400));
    }
}