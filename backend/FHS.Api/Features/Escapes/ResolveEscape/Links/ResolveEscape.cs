using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Escapes.Links;

[Requires(nameof(ResolveEscapeState.Actor), nameof(ResolveEscapeState.Escape))]
public sealed class ResolveEscape(FhsCommandDbContext db, TimeProvider clock) : ILink<ResolveEscapeState>
{
    public ValueTask<LinkResult> RunAsync(ResolveEscapeState state, CancellationToken ct)
    {
        var escape = state.Escape!;
        var now = clock.GetUtcNow();

        escape.Resolution = state.Request.Resolution;
        escape.ResolvedBy = state.Actor.Id;
        escape.ResolvedAt = now;

        db.Set<Escape>().Update(escape);

        state.Events.Add(new EscapeResolved(escape.Id, state.Actor.Id, now));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
