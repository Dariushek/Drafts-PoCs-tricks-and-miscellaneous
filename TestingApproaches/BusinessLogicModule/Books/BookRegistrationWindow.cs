namespace BusinessLogicModule.Books;

internal sealed class BookRegistrationWindow
{
    private BookRegistrationWindow() { }

    public int Id { get; private init; }

    public bool IsOpen { get; private set; }

    public static BookRegistrationWindow Opened(int id) => new () { Id = id, IsOpen = true };

    internal static BookRegistrationWindow FromPersistence(int id, bool isOpen) => new () { Id = id, IsOpen = isOpen };

    public void Close() { IsOpen = false; }
}