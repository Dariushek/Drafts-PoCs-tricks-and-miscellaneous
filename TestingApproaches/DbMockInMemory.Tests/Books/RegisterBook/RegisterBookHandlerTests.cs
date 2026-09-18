using BusinessLogicModule.Books;
using BusinessLogicModule.Books.RegisterBook;

namespace DbMockInMemory.Tests.Books.RegisterBook;

public class RegisterBookHandlerTests
{
    [Fact]
    public void Handle_ValidCommand_ReturnsResultWithNewBookId()
    {
        var handler = new RegisterBookHandler(new BookCatalog());
        var command = new RegisterBookCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        var result = handler.Handle(command);

        Assert.NotEqual(Guid.Empty, result.BookId);
    }

    [Fact]
    public void Handle_ValidCommand_AddsBookToCatalog()
    {
        var catalog = new BookCatalog();
        var handler = new RegisterBookHandler(catalog);
        var command = new RegisterBookCommand("978-0-13-468599-1", "Clean Code", "Robert C. Martin", 3);

        var result = handler.Handle(command);

        var book = catalog.Find(result.BookId);
        Assert.NotNull(book);
        Assert.Equal(command.Isbn, book.Isbn);
        Assert.Equal(command.Title, book.Title);
        Assert.Equal(command.Author, book.Author);
        Assert.Equal(command.CopiesAvailable, book.CopiesAvailable);
    }
}
