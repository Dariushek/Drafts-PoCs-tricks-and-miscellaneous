using System.Collections.Concurrent;

namespace BusinessLogicModule.Books;

internal sealed class BookCatalog
{
    private readonly ConcurrentDictionary<Guid, Book> _books = new();

    public void Add(Book book) => _books[book.Id] = book;

    public Book? Find(Guid id) => _books.GetValueOrDefault(id);
}
