using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Defects.Links;

[Produces(nameof(CreateDefectState.Station))]
public sealed class LoadAndEnsureStationExistence(FhsCommandDbContext db) : ILink<CreateDefectState>
{
    public async ValueTask<LinkResult> RunAsync(CreateDefectState state, CancellationToken ct)
    {
        var stationCode = state.Request.StationCode;

        state.Station = await db.Set<Station>()
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Code == stationCode, ct);

        if (state.Station is null)
        {
            return LinkResult.Fail(DefectErrors.StationNotFound(stationCode));
        }

        if (!state.Station.IsActive)
        {
            return LinkResult.Fail(DefectErrors.StationInactive(stationCode));
        }

        return LinkResult.Continue;
    }
}
