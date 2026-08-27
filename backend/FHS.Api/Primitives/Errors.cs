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
}
