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

    public static Error FacilityNotFound(string code) =>
        new("Escapes.FacilityNotFound", $"No facility is registered with code '{code}'.", ErrorKind.NotFound);

    public static Error FacilityInactive(string code) =>
        new(
            "Escapes.FacilityInactive",
            $"Facility '{code}' is deactivated and cannot accept new escapes.",
            ErrorKind.Conflict
        );

    public static Error FacilityRequired() =>
        new(
            "Escapes.FacilityRequired",
            "This account belongs to no facility, so the escape must name one.",
            ErrorKind.Validation
        );
}
