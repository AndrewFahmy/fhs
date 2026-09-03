using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Stations;

[Requires(nameof(DecommissionStationState.Actor), nameof(DecommissionStationState.Station))]
public sealed class MarkDecommissioned(FhsCommandDbContext db, TimeProvider clock)
    : ILink<DecommissionStationState>
{
    public ValueTask<LinkResult> RunAsync(DecommissionStationState state, CancellationToken ct)
    {
        var station = state.Station!;
        station.IsActive = false;

        db.Set<Station>().Update(station);

        state.Events.Add(new StationDecommissioned(station.Id, state.Actor.Id, clock.GetUtcNow()));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
