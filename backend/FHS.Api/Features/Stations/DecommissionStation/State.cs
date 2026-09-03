using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Stations;

public sealed class DecommissionStationState(Guid stationId) : ChainState, IHasActor, IRaisesEvents
{
    public Guid StationId { get; } = stationId;

    public Actor Actor { get; set; } = null!;

    public List<IDomainEvent> Events { get; } = [];

    public Station? Station { get; set; }
}
