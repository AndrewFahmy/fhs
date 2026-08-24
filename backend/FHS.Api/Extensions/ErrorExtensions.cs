using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Extensions;

public static class ErrorExtensions
{
    public static IResult ToProblem(this Error error)
    {
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };
        if (error.Fields is { Count: > 0 })
            extensions["errors"] = error.Fields;

        return Results.Problem(
            title: error.Message,
            statusCode: StatusCode(error.Kind),
            extensions: extensions
        );
    }

    private static int StatusCode(ErrorKind kind) =>
        kind switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ErrorKind.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError,
        };
}
