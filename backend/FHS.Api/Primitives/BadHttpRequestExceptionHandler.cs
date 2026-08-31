using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace FHS.Api.Primitives;

/// <summary>
/// Turns model-binding failures — malformed JSON, a value the deserializer cannot map onto the
/// request type, a missing body, an unsupported content type — into the same ProblemDetails shape
/// every chain failure produces. Without it they surface as 500s: RouteHandlerOptions
/// .ThrowOnBadRequest defaults to IsDevelopment(), and nothing else catches what it throws.
/// </summary>
public sealed class BadHttpRequestExceptionHandler(ILogger<BadHttpRequestExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not BadHttpRequestException badRequest)
            return false;

        logger.LogDebug(
            badRequest,
            "Rejected a malformed {Method} to {Path}.",
            httpContext.Request.Method,
            httpContext.Request.Path
        );

        var path = (badRequest.InnerException as JsonException)?.Path;
        var error = Errors.MalformedRequest(Describe(path));

        var problem = Results.Problem(
            title: error.Message,
            statusCode: badRequest.StatusCode,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["path"] = path }
        );

        await problem.ExecuteAsync(httpContext);

        return true;
    }

    /// <summary>
    /// The JSON path is safe to return and tells the caller which member was wrong; the exception's
    /// own message names internal parameter and type names, so it stays in the log.
    /// </summary>
    private static string Describe(string? path) =>
        path is not null
            ? $"The request body could not be read at '{path}'."
            : "The request body could not be read.";
}
