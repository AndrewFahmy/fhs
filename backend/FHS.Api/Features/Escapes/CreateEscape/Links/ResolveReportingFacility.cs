using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Escapes.Links;

[Requires(nameof(CreateEscapeState.Actor))]
[Produces(nameof(CreateEscapeState.Facility))]
public sealed class ResolveReportingFacility(FhsCommandDbContext db) : ILink<CreateEscapeState>
{
    public async ValueTask<LinkResult> RunAsync(CreateEscapeState state, CancellationToken ct)
    {
        var facilities = db.Set<Facility>().AsNoTracking();
        Facility? facility = null;

        if (state.Request.FacilityCode is { Length: > 0 } facilityCode)
        {
            facility = await facilities.SingleOrDefaultAsync(f => f.Code == facilityCode, ct);

            if (facility is null)
            {
                return LinkResult.Fail(EscapeErrors.FacilityNotFound(facilityCode));
            }
        }
        else if (state.Actor.FacilityId is { } actorFacilityId)
        {
            // A foreign key guarantees this row exists, so a miss is a data bug rather than a
            // business outcome — SingleAsync lets it reach the exception handler.
            facility = await facilities.SingleAsync(f => f.Id == actorFacilityId, ct);
        }
        else
        {
            return LinkResult.Fail(EscapeErrors.FacilityRequired());
        }

        if (!facility.IsActive)
        {
            return LinkResult.Fail(EscapeErrors.FacilityInactive(facility.Code));
        }

        state.Facility = facility;

        return LinkResult.Continue;
    }
}
