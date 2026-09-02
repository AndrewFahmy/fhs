using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.ErrorCodes.Links;

public sealed class QueryErrorCodes(FhsQueryDbContext db) : ILink<GetErrorCodesState>
{
    public async ValueTask<LinkResult> RunAsync(GetErrorCodesState state, CancellationToken ct)
    {
        IQueryable<ErrorCode> errorCodes = db.Set<ErrorCode>();

        if (!state.IncludeInactive)
        {
            errorCodes = errorCodes.Where(e => e.IsActive);
        }

        var items = await errorCodes
            .OrderBy(e => e.Code)
            .Select(e => new ErrorCodeListItem(e.Id, e.Code, e.Description, e.Severity, e.IsActive))
            .ToListAsync(ct);

        state.Produce(items);

        return LinkResult.Continue;
    }
}
