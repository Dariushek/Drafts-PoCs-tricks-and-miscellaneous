using BusinessLogicModule.Persistence;

namespace BusinessLogicModule.Books;

public interface IClosingRegistrationHandler
{
    Task Handle(ClosingRegistrationCommand command, CancellationToken cancellationToken);
}

internal sealed class ClosingRegistrationHandler(IBookRepository repository) : IClosingRegistrationHandler
{
    public async Task Handle(ClosingRegistrationCommand command, CancellationToken cancellationToken)
    {
        BookRegistrationWindow window = await repository.GetRegistrationWindowAsync(cancellationToken);

        window.Close();

        await repository.SaveRegistrationWindowAsync(window, cancellationToken);
    }
}