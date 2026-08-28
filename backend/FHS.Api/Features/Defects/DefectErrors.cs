using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Defects;

public static class DefectErrors
{
    public static Error StationNotFound(string code) =>
        new("Defects.StationNotFound", $"No station is registered with code '{code}'.", ErrorKind.NotFound);

    public static Error StationInactive(string code) =>
        new(
            "Defects.StationInactive",
            $"Station '{code}' is decommissioned and cannot accept defects.",
            ErrorKind.Conflict
        );

    public static Error ErrorCodeNotFound(string code) =>
        new(
            "Defects.ErrorCodeNotFound",
            $"No error code is registered with code '{code}'.",
            ErrorKind.NotFound
        );

    public static Error ErrorCodeInactive(string code) =>
        new(
            "Defects.ErrorCodeInactive",
            $"Error code '{code}' is retired and cannot classify new defects.",
            ErrorKind.Conflict
        );
}
