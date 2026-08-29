using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Defects.Links;

[Requires(nameof(ResolveDefectState.Actor), nameof(ResolveDefectState.Defect))]
public sealed class MarkResolved(FhsCommandDbContext db, TimeProvider clock) : ILink<ResolveDefectState>
{
    public ValueTask<LinkResult> RunAsync(ResolveDefectState state, CancellationToken ct)
    {
        var defect = state.Defect!;
        var now = clock.GetUtcNow();

        defect.Resolution = state.Request.Resolution;
        defect.ResolvedBy = state.Actor.Id;
        defect.ResolvedAt = now;

        db.Set<Defect>().Update(defect);

        state.Events.Add(new DefectResolved(defect.Id, state.Actor.Id, now));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
