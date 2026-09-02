using FHS.Api.Enums;

namespace FHS.Api.Features.Defects;

public sealed record DefectDetailResponse(
    Guid DefectId,
    Guid StationId,
    string StationCode,
    string StationName,
    Guid ErrorCodeId,
    string ErrorCode,
    string ErrorCodeDescription,
    Severity Severity,
    string Description,
    string RaisedBy,
    DateTimeOffset CreatedAt,
    string? Resolution,
    string? ResolvedBy,
    DateTimeOffset? ResolvedAt
);
