using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.ErrorCodes.Links;

[Requires(nameof(RetireErrorCodeState.Actor), nameof(RetireErrorCodeState.ErrorCode))]
public sealed class MarkErrorCodeAsRetired(FhsCommandDbContext db, TimeProvider clock)
    : ILink<RetireErrorCodeState>
{
    public ValueTask<LinkResult> RunAsync(RetireErrorCodeState state, CancellationToken ct)
    {
        var errorCode = state.ErrorCode!;
        errorCode.IsActive = false;

        db.Set<ErrorCode>().Update(errorCode);

        state.Events.Add(new ErrorCodeRetired(errorCode.Id, state.Actor.Id, clock.GetUtcNow()));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
