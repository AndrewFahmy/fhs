using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Stations.Links;

[Produces(nameof(CreateStationState.Facility))]
public sealed class LoadAndEnsureFacilityExistence(FhsCommandDbContext db) : ILink<CreateStationState>
{
    public async ValueTask<LinkResult> RunAsync(CreateStationState state, CancellationToken ct)
    {
        var facilityCode = state.Request.FacilityCode;

        var facility = await db.Set<Facility>()
            .AsNoTracking()
            .SingleOrDefaultAsync(f => f.Code == facilityCode, ct);

        if (facility is null)
        {
            return LinkResult.Fail(StationErrors.FacilityNotFound(facilityCode));
        }

        if (!facility.IsActive)
        {
            return LinkResult.Fail(StationErrors.FacilityInactive(facilityCode));
        }

        state.Facility = facility;

        return LinkResult.Continue;
    }
}
