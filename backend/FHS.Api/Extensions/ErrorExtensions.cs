using FHS.Api.Primitives;
using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Extensions;

public static class ErrorExtensions
{
    public static IResult ToProblem(this Error error) =>
        Results.Problem(
            new FhsProblemDetails
            {
                Title = error.Message,
                Status = StatusCode(error.Kind),
                Code = error.Code,
                Errors = error.Fields is { Count: > 0 } fields ? fields : null
            }
        );

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
