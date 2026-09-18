using BusinessLogicModule.Books;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicModule.Persistence;

public sealed class BooksDbContext(DbContextOptions<BooksDbContext> options) : DbContext(options)
{
    internal DbSet<Book> Books => Set<Book>();

    internal DbSet<BookRegistrationWindow> RegistrationWindow => Set<BookRegistrationWindow>();

    private const int RegistrationWindowId = 1;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(book =>
        {
            book.HasKey(b => b.Id);
            book.HasIndex(b => b.Isbn).IsUnique();
        });

        modelBuilder.Entity<BookRegistrationWindow>(window =>
        {
            window.HasKey(w => w.Id);
            window.HasData(BookRegistrationWindow.Opened(RegistrationWindowId));
        });
    }
}