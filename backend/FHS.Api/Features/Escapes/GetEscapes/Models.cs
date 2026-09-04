using FHS.Api.Enums;

namespace FHS.Api.Features.Escapes;

public sealed record GetEscapesRequest(
    string? CustomerCode,
    string? ErrorCode,
    Severity? Severity,
    bool? IsResolved,
    int? Page,
    int? PageSize
);

public sealed record EscapeListItem(
    Guid EscapeId,
    string CustomerCode,
    string ErrorCode,
    Severity Severity,
    string Description,
    string ReportedBy,
    DateTimeOffset ReportedAt,
    DateTimeOffset? ResolvedAt
);
