using BusinessLogicModule.Books;
using BusinessLogicModule.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicModule;

public static class ModuleExtensions
{
    public static IServiceCollection AddBusinessLogicModule(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<BooksDbContext>(configureDbContext);
        services.AddTransient<IBookRegistrationHandler, BookRegistrationHandler>();
        services.AddTransient<IClosingRegistrationHandler, ClosingRegistrationHandler>();

        return services;
    }

    public static void MapBusinessLogicModule(this IEndpointRouteBuilder app)
    {
        app.MapBookRegistration();
        app.MapClosingRegistration();
    }

    public static void InitializeBusinessLogicModuleDatabase(this IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();

        scope.ServiceProvider.GetRequiredService<BooksDbContext>().Database.EnsureCreated();
    }
}