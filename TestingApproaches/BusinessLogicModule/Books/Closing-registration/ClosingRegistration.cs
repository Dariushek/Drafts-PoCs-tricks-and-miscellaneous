using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static class ClosingRegistration
{
    public static void MapClosingRegistration(this IEndpointRouteBuilder app)
    {
        app.MapPost("/books/registration/close", Handle).WithName("ClosingRegistration");
    }

    private static async Task<NoContent> Handle(IClosingRegistrationHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(new ClosingRegistrationCommand(), cancellationToken);

        return TypedResults.NoContent();
    }
}