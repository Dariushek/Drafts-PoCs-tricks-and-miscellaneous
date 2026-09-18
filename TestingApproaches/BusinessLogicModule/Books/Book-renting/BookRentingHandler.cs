using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Books;

public interface IBookRentingHandler
{
    Task<Result<BookRentingResult>> Handle(BookRentingCommand command, CancellationToken cancellationToken);
}

internal sealed class BookRentingHandler(BooksDbContext db) : IBookRentingHandler
{
    public async Task<Result<BookRentingResult>> Handle(BookRentingCommand command, CancellationToken cancellationToken)
    {
        Book? book = await db.Books.SingleOrDefaultAsync(book => book.Id == command.BookId, cancellationToken);

        if (book is null)
        {
            return Result<BookRentingResult>.Failure(
                Error.Domain("BookNotFound", $"No book found with id '{command.BookId}'.", StatusCodes.Status404NotFound)
            );
        }

        if (!book.IsAvailableToRent)
        {
            return Result<BookRentingResult>.Failure(
                Error.Domain("BookOutOfStock", $"Book '{book.Title}' has no copies available to rent.", StatusCodes.Status409Conflict)
            );
        }

        book.Rent();
        await db.SaveChangesAsync(cancellationToken);

        return Result<BookRentingResult>.Success(new BookRentingResult(book.Id, book.CopiesAvailable));
    }
}
