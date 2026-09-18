using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule;

public enum ErrorType
{
    Validation,
    Domain
}

public sealed record Error(ErrorType Type, string Code, string Message, string? Field = null, int StatusCode = StatusCodes.Status400BadRequest)
{
    public static Error Validation(string field, string message)
    {
        return new Error(ErrorType.Validation, field, message, field);
    }

    public static Error Domain(string code, string message, int statusCode)
    {
        return new Error(ErrorType.Domain, code, message, StatusCode: statusCode);
    }
}