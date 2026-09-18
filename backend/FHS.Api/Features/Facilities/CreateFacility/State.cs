using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Facilities;

public sealed class CreateFacilityState(CreateFacilityRequest request)
    : ChainState<CreateFacilityResponse>,
        IHasRequest<CreateFacilityRequest>
{
    public CreateFacilityRequest Request { get; } = request;
}
