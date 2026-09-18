namespace BusinessLogicModule.Books;

internal sealed class Book
{
    public Guid Id { get; private init; }

    public string Isbn { get; private init; } = string.Empty;

    public string Title { get; private init; } = string.Empty;

    public string Author { get; private init; } = string.Empty;

    public int CopiesAvailable { get; private set; }

    private Book()
    {
    }

    public static Book Register(string isbn, string title, string author, int copiesAvailable) => new()
    {
        Id = Guid.NewGuid(),
        Isbn = isbn,
        Title = title,
        Author = author,
        CopiesAvailable = copiesAvailable
    };

    public bool IsAvailableToRent => CopiesAvailable > 0;

    public void Rent()
    {
        if (!IsAvailableToRent)
            throw new InvalidOperationException($"Book '{Title}' has no copies available to rent.");

        CopiesAvailable--;
    }
}
