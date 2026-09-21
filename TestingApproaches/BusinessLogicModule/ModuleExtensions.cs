using BusinessLogicModule.Books;
using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BusinessLogicModule;

public static class ModuleExtensions
{
    public static IServiceCollection AddBusinessLogicModule(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<BooksDbContext>(configureDbContext);
        services.AddScoped<IBookRepository, EfBookRepository>();

        return services.AddBookHandlers();
    }

    // Raw ADO.NET against SQL Server, no EF involved for reads/writes. Schema
    // is still provisioned via EF (InitializeBusinessLogicModuleDatabase), so
    // it stays in one place - see PlainSqlBookRepository for why.
    public static IServiceCollection AddBusinessLogicModuleWithPlainSql(this IServiceCollection services, string connectionString, Action<DbContextOptionsBuilder> configureSchemaContext)
    {
        services.AddDbContext<BooksDbContext>(configureSchemaContext);
        services.AddScoped<IBookRepository>(_ => new PlainSqlBookRepository(connectionString));

        return services.AddBookHandlers();
    }

    // Hand-rolled fake, no database at all - the "don't hit anything real"
    // counterpart to the EF InMemory provider.
    public static IServiceCollection AddBusinessLogicModuleWithFakeRepository(this IServiceCollection services)
    {
        services.AddSingleton<IBookRepository, InMemoryFakeBookRepository>();

        return services.AddBookHandlers();
    }

    // Undoes whichever AddBusinessLogicModule* above was already applied
    // (e.g. by Program.cs, inside a WebApplicationFactory) so a test can pick
    // a different persistence approach for the same handlers.
    public static IServiceCollection RemoveBusinessLogicModule(this IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<BooksDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<BooksDbContext>>();
        services.RemoveAll<BooksDbContext>();
        services.RemoveAll<IBookRepository>();
        services.RemoveAll<IBookRegistrationHandler>();
        services.RemoveAll<IClosingRegistrationHandler>();
        services.RemoveAll<IBookAvailabilityHandler>();
        services.RemoveAll<IBookRentingHandler>();

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
    }

    private static IServiceCollection AddBookHandlers(this IServiceCollection services)
    {
        services.AddTransient<IBookRegistrationHandler, BookRegistrationHandler>();
        services.AddTransient<IClosingRegistrationHandler, ClosingRegistrationHandler>();
        services.AddTransient<IBookAvailabilityHandler, BookAvailabilityHandler>();
        services.AddTransient<IBookRentingHandler, BookRentingHandler>();

        return services;
    }
}