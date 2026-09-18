using BusinessLogicModule.Books;
using BusinessLogicModule.Books.RegisterBook;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicModule;

public static class ModuleExtensions
{
    public static IServiceCollection AddBusinessLogicModule(this IServiceCollection services)
    {
        services.AddSingleton<BookCatalog>();
        services.AddTransient<RegisterBookHandler>();

        return services;
    }

    public static void MapBusinessLogicModule(this IEndpointRouteBuilder app)
    {
        app.MapRegisterBook();
    }
}
