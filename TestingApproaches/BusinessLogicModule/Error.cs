using Microsoft.AspNetCore.Http;

namespace BusinessLogicModule;

public sealed record Error(string Code, string Message, string? Field = null, int StatusCode = StatusCodes.Status400BadRequest);
