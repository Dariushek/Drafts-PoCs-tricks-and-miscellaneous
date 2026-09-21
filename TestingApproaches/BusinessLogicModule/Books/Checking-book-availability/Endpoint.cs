using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static partial class Endpoint
{
    public static void MapBookAvailability(this IEndpointRouteBuilder app)
    {
        app.MapGet("/books/{bookId:guid}/availability", HandleAvailability).WithName("BookAvailability");
    }

    private static async Task<Results<ProblemHttpResult, Ok<BookAvailability>>> HandleAvailability
    (
        Guid bookId,
        ICheckBookAvailabilityHandler handler,
        CancellationToken cancellationToken
    )
    {
        Result<BookAvailability> result = await handler.Handle(new(bookId), cancellationToken);

        return result.Match<Results<ProblemHttpResult, Ok<BookAvailability>>>(
            value => TypedResults.Ok(value),
            MapAvailabilityErrors
        );
    }

    private static Results<ProblemHttpResult, Ok<BookAvailability>> MapAvailabilityErrors(IReadOnlyList<Error> errors)
    {
        Error first = errors[0];

        return TypedResults.Problem(title: first.Code, detail: first.Message, statusCode: first.StatusCode);
    }
}