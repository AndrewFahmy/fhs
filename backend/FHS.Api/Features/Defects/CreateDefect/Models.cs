using FHS.Api.Enums;
using FHS.Api.Interfaces;

namespace FHS.Api.Features.Defects;

public sealed record CreateDefectRequest(string StationCode, string ErrorCode, string Description);

public sealed record CreateDefectResponse(Guid DefectId);

public sealed record Classification(Guid ErrorCodeId, Severity Severity);

public sealed record DefectRaised(
    Guid DefectId,
    Guid StationId,
    Guid ErrorCodeId,
    Severity Severity,
    DateTimeOffset OccurredAt
) : IDomainEvent;
