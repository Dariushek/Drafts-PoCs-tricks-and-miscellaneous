using BusinessLogicModule.Books;

namespace BusinessLogicModule.Persistence;

internal interface IBookRepository
{
    Task<BookRegistrationWindow> GetRegistrationWindowAsync(CancellationToken cancellationToken);

    Task SaveRegistrationWindowAsync(BookRegistrationWindow window, CancellationToken cancellationToken);

    Task<bool> IsIsbnRegisteredAsync(string isbn, CancellationToken cancellationToken);

    Task AddAsync(Book book, CancellationToken cancellationToken);

    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(Book book, CancellationToken cancellationToken);
}