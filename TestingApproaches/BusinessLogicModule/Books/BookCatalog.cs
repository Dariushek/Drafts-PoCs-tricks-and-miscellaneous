using System.Collections.Concurrent;

namespace BusinessLogicModule.Books;

internal sealed class BookCatalog
{
    private readonly ConcurrentDictionary<Guid, Book> _books = new();

    public bool IsRegistrationOpen { get; set; } = true;

    public void Add(Book book)
    {
        _books[book.Id] = book;
    }

    public Book? Find(Guid id)
    {
        return _books.GetValueOrDefault(id);
    }

    public bool IsIsbnRegistered(string isbn)
    {
        return _books.Values.Any(book => book.Isbn == isbn);
    }
}