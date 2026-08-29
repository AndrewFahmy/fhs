using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Defects.Links;

[Produces(nameof(ResolveDefectState.Defect))]
public sealed class LoadAndEnsureDefectIsOpen(FhsCommandDbContext db) : ILink<ResolveDefectState>
{
    public async ValueTask<LinkResult> RunAsync(ResolveDefectState state, CancellationToken ct)
    {
        var defect = await db.Set<Defect>()
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == state.DefectId, ct);

        if (defect is null)
        {
            return LinkResult.Fail(DefectErrors.DefectNotFound(state.DefectId));
        }

        if (defect.ResolvedAt is not null)
        {
            return LinkResult.Fail(DefectErrors.DefectAlreadyResolved(state.DefectId));
        }

        state.Defect = defect;

        return LinkResult.Continue;
    }
}
