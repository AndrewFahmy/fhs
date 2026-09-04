using FHS.Api.Interfaces;
using FHS.Api.Primitives;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Escapes;

public sealed class GetEscapesState(GetEscapesRequest request)
    : ChainState<PagedResponse<EscapeListItem>>,
        IHasRequest<GetEscapesRequest>
{
    public GetEscapesRequest Request { get; } = request;
}
