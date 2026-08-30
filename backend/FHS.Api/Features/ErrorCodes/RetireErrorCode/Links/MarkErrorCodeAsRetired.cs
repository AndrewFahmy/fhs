using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.ErrorCodes.Links;

[Requires(nameof(RetireErrorCodeState.ErrorCode))]
public sealed class MarkErrorCodeAsRetired(FhsCommandDbContext db) : ILink<RetireErrorCodeState>
{
    public ValueTask<LinkResult> RunAsync(RetireErrorCodeState state, CancellationToken ct)
    {
        var errorCode = state.ErrorCode!;
        errorCode.IsActive = false;

        db.Set<ErrorCode>().Update(errorCode);

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
