using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Escapes.Links;

[Produces(nameof(CreateEscapeState.Classification))]
public sealed class ClassifyEscape(FhsCommandDbContext db) : ILink<CreateEscapeState>
{
    public async ValueTask<LinkResult> RunAsync(CreateEscapeState state, CancellationToken ct)
    {
        var code = state.Request.ErrorCode;

        var errorCode = await db.Set<ErrorCode>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Code == code, ct);

        if (errorCode is null)
        {
            return LinkResult.Fail(EscapeErrors.ErrorCodeNotFound(code));
        }

        if (!errorCode.IsActive)
        {
            return LinkResult.Fail(EscapeErrors.ErrorCodeInactive(code));
        }

        state.Classification = new Classification(errorCode.Id, errorCode.Severity);

        return LinkResult.Continue;
    }
}
