using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Stations.Links;

[Requires(nameof(CreateStationState.Facility))]
public sealed class AddStation(FhsCommandDbContext db) : ILink<CreateStationState>
{
    public ValueTask<LinkResult> RunAsync(CreateStationState state, CancellationToken ct)
    {
        Station station =
            new()
            {
                FacilityId = state.Facility!.Id,
                Code = state.Request.Code,
                Name = state.Request.Name
            };

        db.Set<Station>().Add(station);
        state.Produce(new CreateStationResponse(station.Id));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
