using FHS.Api.Data;
using FHS.Api.Domains;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Defects.Links;

[Produces(nameof(CreateDefectState.Classification))]
public sealed class ClassifyDefect(FhsCommandDbContext db) : ILink<CreateDefectState>
{
    public async ValueTask<LinkResult> RunAsync(CreateDefectState state, CancellationToken ct)
    {
        var code = state.Request.ErrorCode;

        var errorCode = await db.Set<ErrorCode>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Code == code, ct);

        if (errorCode is null)
        {
            return LinkResult.Fail(DefectErrors.ErrorCodeNotFound(code));
        }

        if (!errorCode.IsActive)
        {
            return LinkResult.Fail(DefectErrors.ErrorCodeInactive(code));
        }

        state.Classification = new Classification(errorCode.Id, errorCode.Severity);

        return LinkResult.Continue;
    }
}
