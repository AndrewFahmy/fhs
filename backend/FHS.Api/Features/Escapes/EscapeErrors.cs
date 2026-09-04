using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Escapes;

public static class EscapeErrors
{
    public static Error CustomerNotFound(string code) =>
        new("Escapes.CustomerNotFound", $"no customer is registered with code '{code}'.", ErrorKind.NotFound);

    public static Error CustomerInactive(string code) =>
        new(
            "Escapes.CustomerInactive",
            $"Customer '{code}' is deactivated and cannot access escapes.",
            ErrorKind.Conflict
        );

    public static Error ErrorCodeNotFound(string code) =>
        new(
            "Escapes.ErrorCodeNotFound",
            $"No error code is registered with code '{code}'.",
            ErrorKind.NotFound
        );

    public static Error ErrorCodeInactive(string code) =>
        new(
            "Escapes.ErrorCodeInactive",
            $"Error code '{code}' is retired and cannot classify new escapes.",
            ErrorKind.Conflict
        );

    public static Error EscapeNotFound(Guid escapeId) =>
        new("Escapes.NotFound", $"No escape exists with id '{escapeId}'.", ErrorKind.NotFound);

    public static Error EscapeAlreadyResolved(Guid escapeId) =>
        new("Escapes.AlreadyResolved", $"Escape '{escapeId}' has already been resolved.", ErrorKind.Conflict);
}
