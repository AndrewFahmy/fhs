using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Defects;

public sealed class ResolveDefectState(Guid defectId, ResolveDefectRequest request)
    : ChainState,
        IHasRequest<ResolveDefectRequest>,
        IHasActor,
        IRaisesEvents
{
    public Guid DefectId { get; } = defectId;

    public ResolveDefectRequest Request { get; } = request;

    public Actor Actor { get; set; } = null!; // written by resolve actor

    public List<IDomainEvent> Events { get; } = [];

    public Defect? Defect { get; set; } // written by LoadAndEnsureDefectIsOpen
}
