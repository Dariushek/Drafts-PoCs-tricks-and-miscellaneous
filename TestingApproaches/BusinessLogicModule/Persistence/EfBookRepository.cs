using BusinessLogicModule.Books;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Persistence;

internal sealed class EfBookRepository(BooksDbContext db) : IBookRepository
{
    public Task<BookRegistrationWindow> GetRegistrationWindowAsync(CancellationToken cancellationToken) =>
        db.RegistrationWindow.SingleAsync(cancellationToken);

    public Task SaveRegistrationWindowAsync(BookRegistrationWindow window, CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public Task<bool> IsIsbnRegisteredAsync(string isbn, CancellationToken cancellationToken) =>
        db.Books.AnyAsync(book => book.Isbn == isbn, cancellationToken);

    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Books.SingleOrDefaultAsync(book => book.Id == id, cancellationToken);

    public Task SaveAsync(Book book, CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
