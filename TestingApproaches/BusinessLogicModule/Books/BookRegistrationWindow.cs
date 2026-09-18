namespace BusinessLogicModule.Books;

internal sealed class BookRegistrationWindow
{
    public int Id { get; private init; }

    public bool IsOpen { get; private set; }

    private BookRegistrationWindow()
    {
    }

    public static BookRegistrationWindow Opened(int id) => new() { Id = id, IsOpen = true };

    public void Close()
    {
        IsOpen = false;
    }
}
