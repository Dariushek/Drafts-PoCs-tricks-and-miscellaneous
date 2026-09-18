using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static class BookRegistration
{
    public static void MapBookRegistration(this IEndpointRouteBuilder app)
    {
        app.MapPost("/books", Handle).WithName("BookRegistration");
    }

    private static Results<ValidationProblem, ProblemHttpResult, Created<BookRegistrationResult>> Handle(
    BookRegistrationCommand command,
    IBookRegistrationHandler handler
    )
    {
        Result<BookRegistrationResult> result = handler.Handle(command);

        return result.Match<Results<ValidationProblem, ProblemHttpResult, Created<BookRegistrationResult>>>(
            value => TypedResults.Created($"/books/{value.BookId}", value),
            MapErrors
        );
    }

    private static Results<ValidationProblem, ProblemHttpResult, Created<BookRegistrationResult>> MapErrors(IReadOnlyList<Error> errors)
    {
        if (errors[0].Type == ErrorType.Validation)
        {
            return TypedResults.ValidationProblem(
                errors.GroupBy(e => e.Field ?? string.Empty).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray())
            );
        }

        Error first = errors[0];
        string detail = string.Join(" ", errors.Select(e => e.Message));

        return TypedResults.Problem(title: first.Code, detail: detail, statusCode: first.StatusCode);
    }
}