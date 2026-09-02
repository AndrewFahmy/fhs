using FHS.Api.Enums;

namespace FHS.Api.Features.Defects;

public sealed record GetDefectsRequest(
    string? StationCode,
    string? ErrorCode,
    Severity? Severity,
    bool? IsResolved,
    int? Page,
    int? PageSize
);

public sealed record DefectListItem(
    Guid DefectId,
    string StationCode,
    string ErrorCode,
    Severity Severity,
    string Description,
    string RaisedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt
);
