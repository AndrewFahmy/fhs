using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Api.Primitives;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Escapes;

public sealed class QueryEscapes(FhsQueryDbContext db) : ILink<GetEscapesState>
{
    public async ValueTask<LinkResult> RunAsync(GetEscapesState state, CancellationToken ct)
    {
        var request = state.Request;

        IQueryable<Escape> escapes = db.Set<Escape>();

        if (request.CustomerCode is { Length: > 0 } customerCode)
        {
            escapes = escapes.Where(e => e.Customer.Code == customerCode);
        }

        if (request.ErrorCode is { Length: > 0 } errorCode)
        {
            escapes = escapes.Where(e => e.ErrorCode.Code == errorCode);
        }

        if (request.Severity is { } severity)
        {
            escapes = escapes.Where(e => e.Severity == severity);
        }

        if (request.IsResolved is { } isResolved)
        {
            escapes = isResolved
                ? escapes.Where(e => e.ResolvedAt != null)
                : escapes.Where(e => e.ResolvedAt == null);
        }

        var totalCount = await escapes.CountAsync(ct);
        var paging = Paging.From(request.Page, request.PageSize);

        var items = await escapes
            .OrderByDescending(e => e.ReportedAt)
            .ThenByDescending(e => e.Id)
            .Skip(paging.Skip)
            .Take(paging.Size)
            .Select(e => new EscapeListItem(
                e.Id,
                e.Customer.Code,
                e.ErrorCode.Code,
                e.Severity,
                e.Description,
                e.Reporter.DisplayName,
                e.ReportedAt,
                e.ResolvedAt
            ))
            .ToListAsync(ct);

        state.Produce(PagedResponse<EscapeListItem>.From(items, paging, totalCount));

        return LinkResult.Continue;
    }
}
