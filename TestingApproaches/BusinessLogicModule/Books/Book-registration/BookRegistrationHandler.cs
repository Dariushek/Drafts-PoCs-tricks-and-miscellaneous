using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule.Books;

public interface IBookRegistrationHandler
{
    Result<BookRegistrationResult> Handle(BookRegistrationCommand command);
}

internal sealed class BookRegistrationHandler(BookCatalog catalog): IBookRegistrationHandler
{
    public Result<BookRegistrationResult> Handle(BookRegistrationCommand command)
    {
        if (!catalog.IsRegistrationOpen)
        {
            return Result<BookRegistrationResult>.Failure(
                Error.Domain("RegistrationClosed", "Book registration is currently closed.", StatusCodes.Status400BadRequest)
            );
        }

        if (command.CopiesAvailable < 0)
        {
            return Result<BookRegistrationResult>.Failure(
                Error.Validation("CopiesAvailable", "Must be zero or greater.")
            );
        }

        if (catalog.IsIsbnRegistered(command.Isbn))
        {
            return Result<BookRegistrationResult>.Failure(
                Error.Domain("IsbnAlreadyRegistered", $"A book with ISBN '{command.Isbn}' is already registered.", StatusCodes.Status409Conflict)
            );
        }

        var book = new Book(Guid.NewGuid(), command.Isbn, command.Title, command.Author, command.CopiesAvailable);

        catalog.Add(book);

        return Result<BookRegistrationResult>.Success(new BookRegistrationResult(book.Id));
    }
}