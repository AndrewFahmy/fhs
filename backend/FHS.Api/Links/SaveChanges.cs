using FHS.Api.Data;
using FHS.Api.Primitives;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Links;

public sealed class SaveChanges(FhsCommandDbContext db) : ILink<ChainState>
{
    public async ValueTask<LinkResult> RunAsync(ChainState state, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return LinkResult.Fail(Errors.ConcurrencyConflict());
        }

        return LinkResult.Continue;
    }
}
