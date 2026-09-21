using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule.Books;

public interface IBookRentingHandler
{
    Task<Result<BookRentingResult>> Handle(BookRentingCommand command, CancellationToken cancellationToken);
}

internal sealed class BookRentingHandler(IBookRepository repository) : IBookRentingHandler
{
    public async Task<Result<BookRentingResult>> Handle(BookRentingCommand command, CancellationToken cancellationToken)
    {
        Book? book = await repository.GetByIdAsync(command.BookId, cancellationToken);

        if (book is null)
        {
            return Result<BookRentingResult>.Failure(
                new Error("BookNotFound", $"No book found with id '{command.BookId}'.", StatusCode: StatusCodes.Status404NotFound)
            );
        }

        if (!book.IsAvailableToRent)
        {
            return Result<BookRentingResult>.Failure(
                new Error("BookOutOfStock", $"Book '{book.Title}' has no copies available to rent.", StatusCode: StatusCodes.Status409Conflict)
            );
        }

        book.Rent();
        await repository.SaveAsync(book, cancellationToken);

        return Result<BookRentingResult>.Success(new BookRentingResult(book.Id, book.CopiesAvailable));
    }
}
