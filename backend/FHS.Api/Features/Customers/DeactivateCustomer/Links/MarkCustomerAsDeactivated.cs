using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Customers.Links;

[Requires(nameof(DeactivateCustomerState.Actor), nameof(DeactivateCustomerState.Customer))]
public sealed class MarkCustomerAsDeactivated(FhsCommandDbContext db, TimeProvider clock)
    : ILink<DeactivateCustomerState>
{
    public ValueTask<LinkResult> RunAsync(DeactivateCustomerState state, CancellationToken ct)
    {
        var customer = state.Customer!;
        customer.IsActive = false;

        db.Set<Customer>().Update(customer);

        state.Events.Add(new CustomerDeactivated(customer.Id, state.Actor.Id, clock.GetUtcNow()));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
