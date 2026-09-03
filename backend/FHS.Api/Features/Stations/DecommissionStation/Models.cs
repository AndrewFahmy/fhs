using FHS.Api.Interfaces;

namespace FHS.Api.Features.Stations;

public sealed record StationDecommissioned(Guid StationId, Guid DecommissionedBy, DateTimeOffset OccurredAt)
    : IDomainEvent;
