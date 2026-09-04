using FHS.Api.Enums;

namespace FHS.Api.Features.Escapes;

public sealed record EscapeDetailResponse(
    Guid EscapeId,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    Guid ErrorCOdeId,
    string ErrorCode,
    string ErrorCodeDescription,
    Severity Severity,
    string Description,
    string ReportedBy,
    DateTimeOffset ReportedAt,
    string? Resolution,
    string? ResolvedBy,
    DateTimeOffset? ResolvedAt
);
