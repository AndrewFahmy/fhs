using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Escapes.Links;

public sealed class LoadEscapeDetail(FhsQueryDbContext db) : ILink<GetEscapeState>
{
    public async ValueTask<LinkResult> RunAsync(GetEscapeState state, CancellationToken ct)
    {
        var escapeId = state.EscapeId;

        var detail = await db.Set<Escape>()
            .Where(e => e.Id == escapeId)
            .Select(e => new EscapeDetailResponse(
                e.Id,
                e.CustomerId,
                e.Customer.Code,
                e.Customer.Name,
                e.ErrorCodeId,
                e.ErrorCode.Code,
                e.ErrorCode.Description,
                e.Severity,
                e.Description,
                e.Reporter.DisplayName,
                e.ReportedAt,
                e.Resolution,
                e.Resolver == null ? null : e.Resolver.DisplayName,
                e.ResolvedAt
            ))
            .FirstOrDefaultAsync(ct);

        if (detail is null)
        {
            return LinkResult.Fail(EscapeErrors.EscapeNotFound(state.EscapeId));
        }

        state.Produce(detail);

        return LinkResult.Continue;
    }
}
