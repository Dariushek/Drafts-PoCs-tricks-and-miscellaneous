namespace BusinessLogicModule.Books;

public sealed record BookRegistrationCommand(string Isbn, string Title, string Author, int CopiesAvailable);