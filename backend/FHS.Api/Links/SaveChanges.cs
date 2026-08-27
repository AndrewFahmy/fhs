using FHS.Api.Data;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Links;

public sealed class SaveChanges(FhsCommandDbContext db) : ILink<ChainState>
{
    public async ValueTask<LinkResult> RunAsync(ChainState state, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);

        return LinkResult.Continue;
    }
}
