namespace BusinessLogicModule.Books;

internal sealed class Book
{
    private Book() { }

    public Guid Id { get; private init; }

    public string Isbn { get; private init; } = string.Empty;

    public string Title { get; private init; } = string.Empty;

    public string Author { get; private init; } = string.Empty;

    public int CopiesAvailable { get; private set; }

    public bool IsAvailableToRent => CopiesAvailable > 0;

    public static Book Register(string isbn, string title, string author, int copiesAvailable)
        => new()
        {
            Id = Guid.NewGuid(),
            Isbn = isbn,
            Title = title,
            Author = author,
            CopiesAvailable = copiesAvailable
        };

    // For repositories that hydrate a Book from a raw row instead of an EF
    // change tracker (e.g. plain ADO.NET), where there's no other way in.
    internal static Book FromPersistence(Guid id, string isbn, string title, string author, int copiesAvailable)
        => new()
        {
            Id = id,
            Isbn = isbn,
            Title = title,
            Author = author,
            CopiesAvailable = copiesAvailable
        };

    public void Rent()
    {
        if (!IsAvailableToRent)
            throw new InvalidOperationException($"Book '{Title}' has no copies available to rent.");

        CopiesAvailable--;
    }
}