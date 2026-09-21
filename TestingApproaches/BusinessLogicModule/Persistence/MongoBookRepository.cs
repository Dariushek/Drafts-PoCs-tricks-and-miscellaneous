using BusinessLogicModule.Books;
using MongoDB.Driver;

namespace BusinessLogicModule.Persistence;

internal sealed class MongoBookRepository(IMongoDatabase database): IBookRepository
{
    private const int RegistrationWindowId = 1;

    private IMongoCollection<BookDocument> Books => database.GetCollection<BookDocument>("books");

    private IMongoCollection<RegistrationWindowDocument> RegistrationWindows =>
        database.GetCollection<RegistrationWindowDocument>("registrationWindows");

    public async Task<BookRegistrationWindow> GetRegistrationWindowAsync(CancellationToken cancellationToken)
    {
        RegistrationWindowDocument document = await RegistrationWindows
                                                    .Find(w => w.Id == RegistrationWindowId)
                                                    .SingleAsync(cancellationToken);

        return BookRegistrationWindow.FromPersistence(document.Id, document.IsOpen);
    }

    public Task SaveRegistrationWindowAsync(BookRegistrationWindow window, CancellationToken cancellationToken)
    {
        return RegistrationWindows.ReplaceOneAsync(
            w => w.Id == window.Id,
            new() { Id = window.Id, IsOpen = window.IsOpen },
            cancellationToken: cancellationToken
        );
    }

    public async Task<bool> IsIsbnRegisteredAsync(string isbn, CancellationToken cancellationToken)
    {
        return await Books.Find(b => b.Isbn == isbn).AnyAsync(cancellationToken);
    }

    public Task AddAsync
        (Book book, CancellationToken cancellationToken) =>
        Books.InsertOneAsync(ToDocument(book), cancellationToken: cancellationToken);

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        BookDocument? document = await Books.Find(b => b.Id == id).SingleOrDefaultAsync(cancellationToken);

        return document is null
            ? null
            : Book.FromPersistence(
                document.Id,
                document.Isbn,
                document.Title,
                document.Author,
                document.CopiesAvailable
            );
    }

    public Task SaveAsync(Book book, CancellationToken cancellationToken)
    {
        return Books.ReplaceOneAsync(b => b.Id == book.Id, ToDocument(book), cancellationToken: cancellationToken);
    }

    private static BookDocument ToDocument(Book book) =>
        new()
        {
            Id = book.Id,
            Isbn = book.Isbn,
            Title = book.Title,
            Author = book.Author,
            CopiesAvailable = book.CopiesAvailable
        };
}