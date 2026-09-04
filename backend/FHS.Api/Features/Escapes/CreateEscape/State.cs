using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Escapes;

public sealed class CreateEscapeState(CreateEscapeRequest request)
    : ChainState<CreateEscapeResponse>,
        IHasRequest<CreateEscapeRequest>,
        IHasActor,
        IRaisesEvents
{
    public CreateEscapeRequest Request { get; } = request;

    public Actor Actor { get; set; } = null!;

    public List<IDomainEvent> Events { get; } = [];

    public Customer? Customer { get; set; }

    public Classification? Classification { get; set; }
}
