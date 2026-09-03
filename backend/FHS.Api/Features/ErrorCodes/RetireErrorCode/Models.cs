using FHS.Api.Interfaces;

namespace FHS.Api.Features.ErrorCodes;

public sealed record ErrorCodeRetired(Guid ErrorCodeId, Guid RetiredBy, DateTimeOffset OccurredAt)
    : IDomainEvent;
