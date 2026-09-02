using FHS.Api.Interfaces;
using FHS.Api.Primitives;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Defects;

public sealed class GetDefectsState(GetDefectsRequest request)
    : ChainState<PagedResponse<DefectListItem>>,
        IHasRequest<GetDefectsRequest>
{
    public GetDefectsRequest Request { get; } = request;
}
