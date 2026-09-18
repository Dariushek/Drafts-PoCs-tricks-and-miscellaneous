using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books.RegisterBook;

internal static class RegisterBookEndpoint
{
    public static void MapRegisterBook(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/books",
                (RegisterBookCommand command, RegisterBookHandler handler) =>
                {
                    var result = handler.Handle(command);
                    return Results.Created($"/books/{result.BookId}", result);
                }
            )
            .WithName("RegisterBook");
    }
}
