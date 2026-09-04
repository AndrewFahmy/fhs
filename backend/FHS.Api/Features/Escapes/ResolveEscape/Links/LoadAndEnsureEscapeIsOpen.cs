using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Escapes.Links;

[Produces(nameof(ResolveEscapeState.Escape))]
public sealed class LoadAndEnsureEscapeIsOpen(FhsCommandDbContext db) : ILink<ResolveEscapeState>
{
    public async ValueTask<LinkResult> RunAsync(ResolveEscapeState state, CancellationToken ct)
    {
        var escape = await db.Set<Escape>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == state.EscapeId, ct);

        if (escape is null)
        {
            return LinkResult.Fail(EscapeErrors.EscapeNotFound(state.EscapeId));
        }

        if (escape.ResolvedAt is not null)
        {
            return LinkResult.Fail(EscapeErrors.EscapeAlreadyResolved(state.EscapeId));
        }

        state.Escape = escape;

        return LinkResult.Continue;
    }
}
