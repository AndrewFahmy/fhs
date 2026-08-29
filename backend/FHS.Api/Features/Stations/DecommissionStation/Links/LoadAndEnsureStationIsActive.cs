using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Stations.Links;

[Produces(nameof(DecommissionStationState.Station))]
public sealed class LoadAndEnsureStationIsActive(FhsCommandDbContext db) : ILink<DecommissionStationState>
{
    public async ValueTask<LinkResult> RunAsync(DecommissionStationState state, CancellationToken ct)
    {
        var station = await db.Set<Station>()
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == state.StationId, ct);

        if (station is null)
        {
            return LinkResult.Fail(StationErrors.NotFound(state.StationId));
        }

        if (!station.IsActive)
        {
            return LinkResult.Fail(StationErrors.AlreadyDecommissioned(state.StationId));
        }

        state.Station = station;

        return LinkResult.Continue;
    }
}
