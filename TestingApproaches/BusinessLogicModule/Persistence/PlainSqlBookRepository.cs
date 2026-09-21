using BusinessLogicModule.Books;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Persistence;

// Table/column names match the EF model's default conventions (see
// BooksDbContext) since schema provisioning is still done via EF's
// Database.EnsureCreated() - only reads/writes bypass EF here. The
// connection string is read off that same (schema-only) BooksDbContext
// rather than passed separately, so both repository kinds share one
// AddBusinessLogicModule wiring.
internal sealed class PlainSqlBookRepository(BooksDbContext db): IBookRepository
{
    private readonly string connectionString = db.Database.GetConnectionString()!;


    public async Task<BookRegistrationWindow> GetRegistrationWindowAsync(CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, IsOpen FROM RegistrationWindow";

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return BookRegistrationWindow.FromPersistence(reader.GetInt32(0), reader.GetBoolean(1));
    }

    public async Task SaveRegistrationWindowAsync(BookRegistrationWindow window, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE RegistrationWindow SET IsOpen = @IsOpen WHERE Id = @Id";
        command.Parameters.AddWithValue("@IsOpen", window.IsOpen);
        command.Parameters.AddWithValue("@Id", window.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> IsIsbnRegisteredAsync(string isbn, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Books WHERE Isbn = @Isbn";
        command.Parameters.AddWithValue("@Isbn", isbn);

        var count = (int)(await command.ExecuteScalarAsync(cancellationToken))!;

        return count > 0;
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO Books (Id, Isbn, Title, Author, CopiesAvailable)
                              VALUES (@Id, @Isbn, @Title, @Author, @CopiesAvailable)
                              """;
        AddBookParameters(command, book);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Isbn, Title, Author, CopiesAvailable FROM Books WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Book.FromPersistence
            (reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4));
    }

    public async Task SaveAsync(Book book, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE Books SET CopiesAvailable = @CopiesAvailable WHERE Id = @Id";
        command.Parameters.AddWithValue("@CopiesAvailable", book.CopiesAvailable);
        command.Parameters.AddWithValue("@Id", book.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }

    private static void AddBookParameters(SqlCommand command, Book book)
    {
        command.Parameters.AddWithValue("@Id", book.Id);
        command.Parameters.AddWithValue("@Isbn", book.Isbn);
        command.Parameters.AddWithValue("@Title", book.Title);
        command.Parameters.AddWithValue("@Author", book.Author);
        command.Parameters.AddWithValue("@CopiesAvailable", book.CopiesAvailable);
    }
}