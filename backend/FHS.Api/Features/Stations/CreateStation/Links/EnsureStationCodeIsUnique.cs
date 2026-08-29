using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Stations.Links;

public sealed class EnsureStationCodeIsUnique(FhsCommandDbContext db) : ILink<CreateStationState>
{
    public async ValueTask<LinkResult> RunAsync(CreateStationState state, CancellationToken ct)
    {
        var code = state.Request.Code;
        var exists = await db.Set<Station>().AsNoTracking().AnyAsync(s => s.Code == code, ct);

        return exists ? LinkResult.Fail(StationErrors.CodeAlreadyExists(code)) : LinkResult.Continue;
    }
}
