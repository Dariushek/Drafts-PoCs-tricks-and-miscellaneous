using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static class BookAvailability
{
    public static void MapBookAvailability(this IEndpointRouteBuilder app)
    {
        app.MapGet("/books/{bookId:guid}/availability", Handle).WithName("BookAvailability");
    }

    private static async Task<Results<ProblemHttpResult, Ok<BookAvailabilityResult>>> Handle(
    Guid bookId,
    IBookAvailabilityHandler handler,
    CancellationToken cancellationToken
    )
    {
        Result<BookAvailabilityResult> result = await handler.Handle(new BookAvailabilityQuery(bookId), cancellationToken);

        return result.Match<Results<ProblemHttpResult, Ok<BookAvailabilityResult>>>(
            value => TypedResults.Ok(value),
            MapErrors
        );
    }

    private static Results<ProblemHttpResult, Ok<BookAvailabilityResult>> MapErrors(IReadOnlyList<Error> errors)
    {
        Error first = errors[0];

        return TypedResults.Problem(title: first.Code, detail: first.Message, statusCode: first.StatusCode);
    }
}
