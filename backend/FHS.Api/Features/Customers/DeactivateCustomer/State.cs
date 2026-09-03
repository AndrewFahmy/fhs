using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Customers;

public sealed class DeactivateCustomerState(Guid CustomerId) : ChainState, IHasActor, IRaisesEvents
{
    public Guid CustomerId { get; } = CustomerId;

    public Actor Actor { get; set; } = null!;

    public List<IDomainEvent> Events { get; } = [];

    public Customer? Customer { get; set; }
}
