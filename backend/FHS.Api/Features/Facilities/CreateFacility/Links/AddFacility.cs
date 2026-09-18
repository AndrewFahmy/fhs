using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Facilities.Links;

public sealed class AddFacility(FhsCommandDbContext db) : ILink<CreateFacilityState>
{
    public ValueTask<LinkResult> RunAsync(CreateFacilityState state, CancellationToken ct)
    {
        Facility facility = new() { Code = state.Request.Code, Name = state.Request.Name };

        db.Set<Facility>().Add(facility);

        state.Produce(new CreateFacilityResponse(facility.Id));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
