using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static class Endpoint
{
    public static void MapBookAvailability(this IEndpointRouteBuilder app)
    {
        app.MapGet("/books/{bookId:guid}/availability", Handle).WithName("BookAvailability");
    }

    private static async Task<Results<ProblemHttpResult, Ok<BookAvailability>>> Handle(
    Guid bookId,
    ICheckBookAvailabilityHandler handler,
    CancellationToken cancellationToken
    )
    {
        Result<BookAvailability> result = await handler.Handle(new CheckBookAvailability(bookId), cancellationToken);

        return result.Match<Results<ProblemHttpResult, Ok<BookAvailability>>>(
            value => TypedResults.Ok(value),
            MapErrors
        );
    }

    private static Results<ProblemHttpResult, Ok<BookAvailability>> MapErrors(IReadOnlyList<Error> errors)
    {
        Error first = errors[0];

        return TypedResults.Problem(title: first.Code, detail: first.Message, statusCode: first.StatusCode);
    }
}
