using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.ErrorCodes;

public sealed class RetireErrorCodeState(Guid errorCodeId) : ChainState, IHasActor, IRaisesEvents
{
    public Guid ErrorCodeId { get; } = errorCodeId;

    public Actor Actor { get; set; } = null!;

    public List<IDomainEvent> Events { get; } = [];

    public ErrorCode? ErrorCode { get; set; }
}
