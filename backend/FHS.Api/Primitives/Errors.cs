using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Primitives;

public static class Errors
{
    public static Error Unauthenticated() =>
        new("Auth.Unauthenticated", $"The request carries no authenticated subject.", ErrorKind.Forbidden);

    public static Error UnknownActor(string subjectId) =>
        new(
            "Auth.UnknownActor",
            $"Subject '{subjectId}' is not registered as an actor in FHS.",
            ErrorKind.Forbidden
        );

    public static Error ConcurrencyConflict() =>
        new(
            "Concurrency.Conflict",
            "The record was modified by someone else while this request was in flight. Reload and try again.",
            ErrorKind.Conflict
        );

    public static Error MalformedRequest(string message) =>
        new("Request.Malformed", message, ErrorKind.Validation);
}
