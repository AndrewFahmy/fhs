using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Stations;

public static class StationErrors
{
    public static Error CodeAlreadyExists(string code) =>
        new(
            "Stations.CodeAlreadyExists",
            $"A station with code '{code}' already exists.",
            ErrorKind.Conflict
        );

    public static Error NotFound(Guid stationId) =>
        new("Stations.NotFound", $"No station exists with id '{stationId}'.", ErrorKind.NotFound);

    public static Error AlreadyDecommissioned(Guid stationId) =>
        new(
            "Stations.AlreadyDecommissioned",
            $"Station '{stationId}' is already decommissioned.",
            ErrorKind.Conflict
        );
}
