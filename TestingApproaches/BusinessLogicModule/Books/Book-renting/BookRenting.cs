using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static class BookRenting
{
    public static void MapBookRenting(this IEndpointRouteBuilder app)
    {
        app.MapPost("/books/{bookId:guid}/rent", Handle).WithName("BookRenting");
    }

    private static async Task<Results<ProblemHttpResult, Ok<BookRentingResult>>> Handle(
    Guid bookId,
    IBookRentingHandler handler,
    CancellationToken cancellationToken
    )
    {
        Result<BookRentingResult> result = await handler.Handle(new BookRentingCommand(bookId), cancellationToken);

        return result.Match<Results<ProblemHttpResult, Ok<BookRentingResult>>>(
            value => TypedResults.Ok(value),
            MapErrors
        );
    }

    private static Results<ProblemHttpResult, Ok<BookRentingResult>> MapErrors(IReadOnlyList<Error> errors)
    {
        Error first = errors[0];

        return TypedResults.Problem(title: first.Code, detail: first.Message, statusCode: first.StatusCode);
    }
}
