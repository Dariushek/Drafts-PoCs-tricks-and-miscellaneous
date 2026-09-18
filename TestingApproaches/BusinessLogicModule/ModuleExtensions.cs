using BusinessLogicModule.Books;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicModule;

public static class ModuleExtensions
{
    public static IServiceCollection AddBusinessLogicModule(this IServiceCollection services)
    {
        services.AddSingleton<BookCatalog>();
        services.AddTransient<IBookRegistrationHandler, BookRegistrationHandler>();
        services.AddTransient<IClosingRegistrationHandler, ClosingRegistrationHandler>();

        return services;
    }

    public static void MapBusinessLogicModule(this IEndpointRouteBuilder app)
    {
        app.MapBookRegistration();
        app.MapClosingRegistration();
    }
}