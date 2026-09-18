using BusinessLogicModule.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Books;

public interface IClosingRegistrationHandler
{
    Task Handle(ClosingRegistrationCommand command, CancellationToken cancellationToken);
}

internal sealed class ClosingRegistrationHandler(BooksDbContext db) : IClosingRegistrationHandler
{
    public async Task Handle(ClosingRegistrationCommand command, CancellationToken cancellationToken)
    {
        BookRegistrationWindow window = await db.RegistrationWindow.SingleAsync(cancellationToken);

        window.Close();

        await db.SaveChangesAsync(cancellationToken);
    }
}