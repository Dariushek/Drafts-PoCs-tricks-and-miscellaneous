using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static partial class Endpoint
{
    public static void MapBookRenting(this IEndpointRouteBuilder app)
    {
        app.MapPost("/books/{bookId:guid}/rent", HandleRenting)
           .WithName("BookRenting");
    }

    private static async Task<Results<ProblemHttpResult, Ok<BookRenting>>> HandleRenting
        (Guid bookId, IRentBookHandler handler, CancellationToken cancellationToken)
    {
        Result<BookRenting> result = await handler.Handle(new(bookId), cancellationToken);

        return result.Match<Results<ProblemHttpResult, Ok<BookRenting>>>
            (value => TypedResults.Ok(value), MapRentingErrors);
    }

    private static Results<ProblemHttpResult, Ok<BookRenting>> MapRentingErrors(IReadOnlyList<Error> errors)
    {
        Error first = errors[0];

        return TypedResults.Problem(title: first.Code, detail: first.Message, statusCode: first.StatusCode);
    }
}