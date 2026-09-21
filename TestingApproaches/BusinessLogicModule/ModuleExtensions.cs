using BusinessLogicModule.Books;
using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace BusinessLogicModule;

public static class ModuleExtensions
{
    // configureDbContext is required for Ef/PlainSql (both still use EF for
    // schema - see PlainSqlBookRepository for why) and ignored for
    // Fake/Mongo. mongoConnectionString/mongoDatabaseName are required for
    // Mongo only. persistenceKind defaults to Ef, so production call sites
    // (Program.cs) don't need to change to pick up the other kinds becoming
    // available.
    public static IServiceCollection AddBusinessLogicModule
    (
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configureDbContext = null,
        PersistenceKind persistenceKind = PersistenceKind.Ef,
        string? mongoConnectionString = null,
        string? mongoDatabaseName = null
    )
    {
        if (persistenceKind == PersistenceKind.Fake)
        {
            services.AddSingleton<IBookRepository, InMemoryFakeBookRepository>();
            return services.AddBookHandlers();
        }

        if (persistenceKind == PersistenceKind.Mongo)
        {
            ArgumentNullException.ThrowIfNull(mongoConnectionString);
            ArgumentNullException.ThrowIfNull(mongoDatabaseName);

            services.AddSingleton(new MongoClient(mongoConnectionString).GetDatabase(mongoDatabaseName));
            services.AddScoped<IBookRepository, MongoBookRepository>();

            return services.AddBookHandlers();
        }

        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<BooksDbContext>(configureDbContext);
        services.AddScoped<IBookRepository>(
            persistenceKind == PersistenceKind.PlainSql
                ? sp => new PlainSqlBookRepository(sp.GetRequiredService<BooksDbContext>())
                : sp => new EfBookRepository(sp.GetRequiredService<BooksDbContext>())
        );

        return services.AddBookHandlers();
    }

    // Undoes an earlier AddBusinessLogicModule call (e.g. Program.cs's,
    // inside a WebApplicationFactory) so a test can pick a different
    // persistence approach for the same handlers.
    public static IServiceCollection RemoveBusinessLogicModule(this IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<BooksDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<BooksDbContext>>();
        services.RemoveAll<BooksDbContext>();
        services.RemoveAll<IMongoDatabase>();
        services.RemoveAll<IBookRepository>();
        services.RemoveAll<IRegisterBookHandler>();
        services.RemoveAll<ICloseRegistrationHandler>();
        services.RemoveAll<ICheckBookAvailabilityHandler>();
        services.RemoveAll<IRentBookHandler>();

        return services;
    }

    public static void MapBusinessLogicModule(this IEndpointRouteBuilder app)
    {
        app.MapBookRegistration();
        app.MapClosingRegistration();
        app.MapBookAvailability();
        app.MapBookRenting();
    }

    public static void InitializeBusinessLogicModuleDatabase(this IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();

        scope.ServiceProvider.GetService<BooksDbContext>()?.Database.EnsureCreated();

        // Mongo has no schema/HasData equivalent to EnsureCreated - the
        // registration window singleton is seeded explicitly instead.
        // Upsert keeps this idempotent if called more than once.
        scope.ServiceProvider.GetService<IMongoDatabase>()
             ?.GetCollection<RegistrationWindowDocument>("registrationWindows")
             .ReplaceOne(
                 w => w.Id == 1,
                 new() { Id = 1, IsOpen = true },
                 new ReplaceOptions { IsUpsert = true }
             );
    }

    private static IServiceCollection AddBookHandlers(this IServiceCollection services)
    {
        services.AddTransient<IRegisterBookHandler, RegisterBookHandler>();
        services.AddTransient<ICloseRegistrationHandler, CloseRegistrationHandler>();
        services.AddTransient<ICheckBookAvailabilityHandler, CheckBookAvailabilityHandler>();
        services.AddTransient<IRentBookHandler, RentBookHandler>();

        return services;
    }
}