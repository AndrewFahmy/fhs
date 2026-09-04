using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Escapes;

public sealed class ResolveEscapeState(Guid escapeId, ResolveEscapeRequest request)
    : ChainState,
        IHasRequest<ResolveEscapeRequest>,
        IHasActor,
        IRaisesEvents
{
    public Guid EscapeId { get; } = escapeId;

    public ResolveEscapeRequest Request { get; } = request;

    public Actor Actor { get; set; } = null!;

    public List<IDomainEvent> Events { get; } = [];

    public Escape? Escape { get; set; }
}
