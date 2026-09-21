using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule.Books;

public interface IRentBookHandler
{
    Task<Result<BookRenting>> Handle(RentBook command, CancellationToken cancellationToken);
}

internal sealed class RentBookHandler(IBookRepository repository): IRentBookHandler
{
    public async Task<Result<BookRenting>> Handle(RentBook command, CancellationToken cancellationToken)
    {
        Book? book = await repository.GetByIdAsync(command.BookId, cancellationToken);

        if (book is null)
        {
            return Result<BookRenting>.Failure
            (
                new Error
                (
                    "BookNotFound",
                    $"No book found with id '{command.BookId}'.",
                    StatusCode: StatusCodes.Status404NotFound
                )
            );
        }

        if (!book.IsAvailableToRent)
        {
            return Result<BookRenting>.Failure
            (
                new Error
                (
                    "BookOutOfStock",
                    $"Book '{book.Title}' has no copies available to rent.",
                    StatusCode: StatusCodes.Status409Conflict
                )
            );
        }

        book.Rent();
        await repository.SaveAsync(book, cancellationToken);

        return Result<BookRenting>.Success(new(book.Id, book.CopiesAvailable));
    }
}