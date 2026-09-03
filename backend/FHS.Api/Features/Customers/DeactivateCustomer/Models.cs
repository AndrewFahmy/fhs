using FHS.Api.Interfaces;

namespace FHS.Api.Features.Customers;

public sealed record CustomerDeactivated(Guid CustomerId, Guid DeactivatedBy, DateTimeOffset OccurredAt)
    : IDomainEvent;
