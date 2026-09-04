using FHS.Chain.Contracts;

namespace FHS.Api.Features.Escapes;

public sealed class GetEscapeState(Guid escapeId) : ChainState<EscapeDetailResponse>
{
    public Guid EscapeId { get; } = escapeId;
}
