using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule.Books;

public interface IRegisterBookHandler
{
    Task<Result<BookRegistration>> Handle(RegisterBook command, CancellationToken cancellationToken);
}

internal sealed class RegisterBookHandler(IBookRepository repository): IRegisterBookHandler
{
    public async Task<Result<BookRegistration>> Handle(RegisterBook command, CancellationToken cancellationToken)
    {
        BookRegistrationWindow window = await repository.GetRegistrationWindowAsync(cancellationToken);

        if (!window.IsOpen)
        {
            return Result<BookRegistration>.Failure
            (
                new Error
                (
                    "RegistrationClosed",
                    "Book registration is currently closed.",
                    StatusCode: StatusCodes.Status400BadRequest
                )
            );
        }

        if (command.CopiesAvailable < 0)
        {
            return Result<BookRegistration>.Failure
                (new Error("CopiesAvailable", "Must be zero or greater.", "CopiesAvailable"));
        }

        bool isbnRegistered = await repository.IsIsbnRegisteredAsync(command.Isbn, cancellationToken);

        if (isbnRegistered)
        {
            return Result<BookRegistration>.Failure
            (
                new Error
                (
                    "IsbnAlreadyRegistered",
                    $"A book with ISBN '{command.Isbn}' is already registered.",
                    StatusCode: StatusCodes.Status409Conflict
                )
            );
        }

        Book book = Book.Register(command.Isbn, command.Title, command.Author, command.CopiesAvailable);

        await repository.AddAsync(book, cancellationToken);

        return Result<BookRegistration>.Success(new(book.Id));
    }
}