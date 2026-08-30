using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.ErrorCodes.Links;

[Produces(nameof(RetireErrorCodeState.ErrorCode))]
public sealed class LoadAndEnsureErrorCodeIsActive(FhsCommandDbContext db) : ILink<RetireErrorCodeState>
{
    public async ValueTask<LinkResult> RunAsync(RetireErrorCodeState state, CancellationToken ct)
    {
        var errorCode = await db.Set<ErrorCode>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == state.ErrorCodeId, ct);

        if (errorCode is null)
        {
            return LinkResult.Fail(ErrorCodeErrors.NotFound(state.ErrorCodeId));
        }

        if (!errorCode.IsActive)
        {
            return LinkResult.Fail(ErrorCodeErrors.AlreadyRetired(state.ErrorCodeId));
        }

        state.ErrorCode = errorCode;

        return LinkResult.Continue;
    }
}
