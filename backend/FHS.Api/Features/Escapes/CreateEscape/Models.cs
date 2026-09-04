using FHS.Api.Enums;
using FHS.Api.Interfaces;

namespace FHS.Api.Features.Escapes;

public sealed record CreateEscapeRequest(string CustomerCode, string ErrorCode, string Description);

public sealed record CreateEscapeResponse(Guid EscapeId);

public sealed record Classification(Guid ErrorCodeId, Severity Severity);

public sealed record EscapeReported(
    Guid EscapeId,
    Guid CustomerId,
    Guid ErrorCodeId,
    Severity Severity,
    DateTimeOffset OccurredAt
) : IDomainEvent;
