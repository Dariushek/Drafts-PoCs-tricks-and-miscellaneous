using BusinessLogicModule.Persistence;

namespace BusinessLogicModule.Books;

public interface ICloseRegistrationHandler
{
    Task Handle(CloseRegistration command, CancellationToken cancellationToken);
}

internal sealed class CloseRegistrationHandler(IBookRepository repository) : ICloseRegistrationHandler
{
    public async Task Handle(CloseRegistration command, CancellationToken cancellationToken)
    {
        BookRegistrationWindow window = await repository.GetRegistrationWindowAsync(cancellationToken);

        window.Close();

        await repository.SaveRegistrationWindowAsync(window, cancellationToken);
    }
}
