using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BusinessLogicModule.Books;

internal static partial class Endpoint
{
    public static void MapBookRegistration(this IEndpointRouteBuilder app)
    {
        app.MapPost("/books", HandleRegistration).WithName("BookRegistration");
    }

    private static async Task<Results<ValidationProblem, ProblemHttpResult, Created<BookRegistration>>>
        HandleRegistration
        (
            RegisterBook command,
            IRegisterBookHandler handler,
            CancellationToken cancellationToken
        )
    {
        Result<BookRegistration> result = await handler.Handle(command, cancellationToken);

        return result.Match<Results<ValidationProblem, ProblemHttpResult, Created<BookRegistration>>>(
            value => TypedResults.Created($"/books/{value.BookId}", value),
            MapRegistrationErrors
        );
    }

    private static Results<ValidationProblem, ProblemHttpResult, Created<BookRegistration>> MapRegistrationErrors
        (IReadOnlyList<Error> errors)
    {
        if (errors[0].Field is { })
        {
            return TypedResults.ValidationProblem(
                errors.GroupBy(e => e.Field ?? string.Empty)
                      .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray())
            );
        }

        Error first = errors[0];
        string detail = string.Join(" ", errors.Select(e => e.Message));

        return TypedResults.Problem(title: first.Code, detail: detail, statusCode: first.StatusCode);
    }
}