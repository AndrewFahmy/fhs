using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Facilities.Links;

public sealed class EnsureFacilityCodeIsUnique(FhsCommandDbContext db) : ILink<CreateFacilityState>
{
    public async ValueTask<LinkResult> RunAsync(CreateFacilityState state, CancellationToken ct)
    {
        var code = state.Request.Code;
        var exists = await db.Set<Facility>().AsNoTracking().AnyAsync(f => f.Code == code, ct);

        return exists ? LinkResult.Fail(FacilityErrors.CodeAlreadyExists(code)) : LinkResult.Continue;
    }
}
