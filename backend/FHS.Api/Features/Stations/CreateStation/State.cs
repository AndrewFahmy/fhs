using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Stations;

public sealed class CreateStationState(CreateStationRequest request)
    : ChainState<CreateStationResponse>,
        IHasRequest<CreateStationRequest>
{
    public CreateStationRequest Request { get; } = request;
}
