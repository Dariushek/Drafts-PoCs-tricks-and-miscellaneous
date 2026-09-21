using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule.Books;

public interface ICheckBookAvailabilityHandler
{
    Task<Result<BookAvailability>> Handle(CheckBookAvailability query, CancellationToken cancellationToken);
}

internal sealed class CheckBookAvailabilityHandler(IBookRepository repository): ICheckBookAvailabilityHandler
{
    public async Task<Result<BookAvailability>> Handle(CheckBookAvailability query, CancellationToken cancellationToken)
    {
        Book? book = await repository.GetByIdAsync(query.BookId, cancellationToken);

        if (book is null)
        {
            return Result<BookAvailability>.Failure(
                new Error(
                    "BookNotFound",
                    $"No book found with id '{query.BookId}'.",
                    StatusCode: StatusCodes.Status404NotFound
                )
            );
        }

        return Result<BookAvailability>.Success(
            new(book.Id, book.IsAvailableToRent, book.CopiesAvailable)
        );
    }
}