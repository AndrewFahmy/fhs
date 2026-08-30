using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.ErrorCodes.Links;

public sealed class EnsureErrorCodeIsUnique(FhsCommandDbContext db) : ILink<CreateErrorCodeState>
{
    public async ValueTask<LinkResult> RunAsync(CreateErrorCodeState state, CancellationToken ct)
    {
        var code = state.Request.Code;
        var exists = await db.Set<ErrorCode>().AsNoTracking().AnyAsync(e => e.Code == code, ct);

        return exists ? LinkResult.Fail(ErrorCodeErrors.CodeAlreadyExists(code)) : LinkResult.Continue;
    }
}
