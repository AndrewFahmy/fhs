using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Defects;

public sealed class CreateDefectState(CreateDefectRequest request)
    : ChainState<CreateDefectResponse>,
        IHasRequest<CreateDefectRequest>,
        IHasActor,
        IRaisesEvents
{
    public CreateDefectRequest Request { get; } = request;

    public Actor Actor { get; set; } = null!; // written by ResolveActor

    public List<IDomainEvent> Events { get; } = [];

    public Station? Station { get; set; } // written by LoadStation

    public Classification? Classification { get; set; } // written by ClassifyDefect
}
