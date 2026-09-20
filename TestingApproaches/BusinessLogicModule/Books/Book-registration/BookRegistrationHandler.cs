using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Books;

public interface IBookRegistrationHandler
{
    Task<Result<BookRegistrationResult>> Handle(BookRegistrationCommand command, CancellationToken cancellationToken);
}

internal sealed class BookRegistrationHandler(BooksDbContext db): IBookRegistrationHandler
{
    public async Task<Result<BookRegistrationResult>> Handle(BookRegistrationCommand command, CancellationToken cancellationToken)
    {
        BookRegistrationWindow window = await db.RegistrationWindow.SingleAsync(cancellationToken);

        if (!window.IsOpen)
        {
            return Result<BookRegistrationResult>.Failure(
                new Error("RegistrationClosed", "Book registration is currently closed.", StatusCode: StatusCodes.Status400BadRequest)
            );
        }

        if (command.CopiesAvailable < 0)
        {
            return Result<BookRegistrationResult>.Failure(
                new Error("CopiesAvailable", "Must be zero or greater.", "CopiesAvailable")
            );
        }

        bool isbnRegistered = await db.Books.AnyAsync(book => book.Isbn == command.Isbn, cancellationToken);

        if (isbnRegistered)
        {
            return Result<BookRegistrationResult>.Failure(
                new Error("IsbnAlreadyRegistered", $"A book with ISBN '{command.Isbn}' is already registered.", StatusCode: StatusCodes.Status409Conflict)
            );
        }

        Book book = Book.Register(command.Isbn, command.Title, command.Author, command.CopiesAvailable);

        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);

        return Result<BookRegistrationResult>.Success(new BookRegistrationResult(book.Id));
    }
}