using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Api.Primitives;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Defects.Links;

public sealed class QueryDefects(FhsQueryDbContext db) : ILink<GetDefectsState>
{
    public async ValueTask<LinkResult> RunAsync(GetDefectsState state, CancellationToken ct)
    {
        var request = state.Request;

        IQueryable<Defect> defects = db.Set<Defect>();

        if (request.StationCode is { Length: > 0 } stationCode)
        {
            defects = defects.Where(d => d.Station.Code == stationCode);
        }

        if (request.ErrorCode is { Length: > 0 } errorCode)
        {
            defects = defects.Where(d => d.ErrorCode.Code == errorCode);
        }

        if (request.Severity is { } severity)
        {
            defects = defects.Where(d => d.Severity == severity);
        }

        if (request.IsResolved is { } isResolved)
        {
            defects = isResolved
                ? defects.Where(d => d.ResolvedAt != null)
                : defects.Where(d => d.ResolvedAt == null);
        }

        var totalCount = await defects.CountAsync(ct);
        var paging = Paging.From(request.Page, request.PageSize);

        var items = await defects
            .OrderByDescending(d => d.CreatedAt)
            .ThenByDescending(d => d.Id)
            .Skip(paging.Skip)
            .Take(paging.Size)
            .Select(d => new DefectListItem(
                d.Id,
                d.Station.Code,
                d.ErrorCode.Code,
                d.Severity,
                d.Description,
                d.Creator.DisplayName,
                d.CreatedAt,
                d.ResolvedAt
            ))
            .ToListAsync(ct);

        state.Produce(PagedResponse<DefectListItem>.From(items, paging, totalCount));

        return LinkResult.Continue;
    }
}
