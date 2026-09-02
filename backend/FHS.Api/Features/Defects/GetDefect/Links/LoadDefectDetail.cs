using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Defects.Links;

public sealed class LoadDefectDetail(FhsQueryDbContext db) : ILink<GetDefectState>
{
    public async ValueTask<LinkResult> RunAsync(GetDefectState state, CancellationToken ct)
    {
        var defectId = state.DefectId;

        var detail = await db.Set<Defect>()
            .Where(d => d.Id == defectId)
            .Select(d => new DefectDetailResponse(
                d.Id,
                d.StationId,
                d.Station.Code,
                d.Station.Name,
                d.ErrorCodeId,
                d.ErrorCode.Code,
                d.ErrorCode.Description,
                d.Severity,
                d.Description,
                d.Creator.DisplayName,
                d.CreatedAt,
                d.Resolution,
                d.Resolver == null ? null : d.Resolver.DisplayName,
                d.ResolvedAt
            ))
            .FirstOrDefaultAsync(ct);

        if (detail is null)
        {
            return LinkResult.Fail(DefectErrors.DefectNotFound(defectId));
        }

        state.Produce(detail);

        return LinkResult.Continue;
    }
}
