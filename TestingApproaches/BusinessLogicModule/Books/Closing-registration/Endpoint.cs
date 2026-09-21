using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static partial class Endpoint
{
    public static void MapClosingRegistration(this IEndpointRouteBuilder app)
    {
        app.MapPost("/books/registration/close", HandleClosingRegistration).WithName("ClosingRegistration");
    }

    private static async Task<NoContent> HandleClosingRegistration(ICloseRegistrationHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(new CloseRegistration(), cancellationToken);

        return TypedResults.NoContent();
    }
}
