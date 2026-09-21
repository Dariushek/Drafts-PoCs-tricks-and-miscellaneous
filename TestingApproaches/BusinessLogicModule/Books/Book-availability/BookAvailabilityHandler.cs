using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule.Books;

public interface IBookAvailabilityHandler
{
    Task<Result<BookAvailabilityResult>> Handle(BookAvailabilityQuery query, CancellationToken cancellationToken);
}

internal sealed class BookAvailabilityHandler(IBookRepository repository) : IBookAvailabilityHandler
{
    public async Task<Result<BookAvailabilityResult>> Handle(BookAvailabilityQuery query, CancellationToken cancellationToken)
    {
        Book? book = await repository.GetByIdAsync(query.BookId, cancellationToken);

        if (book is null)
        {
            return Result<BookAvailabilityResult>.Failure(
                new Error("BookNotFound", $"No book found with id '{query.BookId}'.", StatusCode: StatusCodes.Status404NotFound)
            );
        }

        return Result<BookAvailabilityResult>.Success(new BookAvailabilityResult(book.Id, book.IsAvailableToRent, book.CopiesAvailable));
    }
}
