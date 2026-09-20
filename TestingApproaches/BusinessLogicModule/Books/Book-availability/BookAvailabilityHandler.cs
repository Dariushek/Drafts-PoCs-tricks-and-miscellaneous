using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Books;

public interface IBookAvailabilityHandler
{
    Task<Result<BookAvailabilityResult>> Handle(BookAvailabilityQuery query, CancellationToken cancellationToken);
}

internal sealed class BookAvailabilityHandler(BooksDbContext db) : IBookAvailabilityHandler
{
    public async Task<Result<BookAvailabilityResult>> Handle(BookAvailabilityQuery query, CancellationToken cancellationToken)
    {
        Book? book = await db.Books.SingleOrDefaultAsync(book => book.Id == query.BookId, cancellationToken);

        if (book is null)
        {
            return Result<BookAvailabilityResult>.Failure(
                new Error("BookNotFound", $"No book found with id '{query.BookId}'.", StatusCode: StatusCodes.Status404NotFound)
            );
        }

        return Result<BookAvailabilityResult>.Success(new BookAvailabilityResult(book.Id, book.IsAvailableToRent, book.CopiesAvailable));
    }
}
