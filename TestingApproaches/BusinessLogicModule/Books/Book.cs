namespace BusinessLogicModule.Books;

internal sealed record Book(Guid Id, string Isbn, string Title, string Author, int CopiesAvailable);