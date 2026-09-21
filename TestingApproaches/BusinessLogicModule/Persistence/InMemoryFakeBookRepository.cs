using System.Collections.Concurrent;
using BusinessLogicModule.Books;

namespace BusinessLogicModule.Persistence;

// Hand-rolled test double: no SQL, no EF, just an in-memory dictionary. Books
// are stored by reference, so mutating one already mutates the stored copy -
// SaveAsync is a no-op.
internal sealed class InMemoryFakeBookRepository: IBookRepository
{
    private const int RegistrationWindowId = 1;

    private readonly ConcurrentDictionary<Guid, Book> books = new();
    private BookRegistrationWindow registrationWindow = BookRegistrationWindow.Opened(RegistrationWindowId);

    public Task<BookRegistrationWindow> GetRegistrationWindowAsync
        (CancellationToken cancellationToken)
        => Task.FromResult(registrationWindow);

    public Task SaveRegistrationWindowAsync(BookRegistrationWindow window, CancellationToken cancellationToken)
    {
        registrationWindow = window;
        return Task.CompletedTask;
    }

    public Task<bool> IsIsbnRegisteredAsync(string isbn, CancellationToken cancellationToken)
    {
        return Task.FromResult(books.Values.Any(book => book.Isbn == isbn));
    }

    public Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        books[book.Id] = book;
        return Task.CompletedTask;
    }

    public Task<Book?> GetByIdAsync
        (Guid id, CancellationToken cancellationToken)
        => Task.FromResult(books.GetValueOrDefault(id));

    public Task SaveAsync(Book book, CancellationToken cancellationToken) => Task.CompletedTask;
}