namespace BusinessLogicModule.Books.RegisterBook;

public sealed record RegisterBookCommand(string Isbn, string Title, string Author, int CopiesAvailable);
