using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Stations;

public sealed class DecommissionStationState(Guid stationId) : ChainState
{
    public Guid StationId { get; } = stationId;

    public Station? Station { get; set; }
}
