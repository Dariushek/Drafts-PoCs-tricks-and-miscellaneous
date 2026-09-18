namespace BusinessLogicModule.Books.RegisterBook;

internal sealed class RegisterBookHandler(BookCatalog catalog)
{
    public RegisterBookResult Handle(RegisterBookCommand command)
    {
        var book = new Book(Guid.NewGuid(), command.Isbn, command.Title, command.Author, command.CopiesAvailable);

        catalog.Add(book);

        return new RegisterBookResult(book.Id);
    }
}
