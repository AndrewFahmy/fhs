using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Stations;

[Requires(nameof(DecommissionStationState.Station))]
public sealed class MarkDecommissioned(FhsCommandDbContext db) : ILink<DecommissionStationState>
{
    public ValueTask<LinkResult> RunAsync(DecommissionStationState state, CancellationToken ct)
    {
        var station = state.Station!;
        station.IsActive = false;

        db.Set<Station>().Update(station);

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
