namespace BusinessLogicModule.Books;

public interface IClosingRegistrationHandler
{
    void Handle(ClosingRegistrationCommand command);
}

internal sealed class ClosingRegistrationHandler(BookCatalog catalog) : IClosingRegistrationHandler
{
    public void Handle(ClosingRegistrationCommand command)
    {
        catalog.IsRegistrationOpen = false;
    }
}