using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Customers.Links;

[Produces(nameof(DeactivateCustomerState.Customer))]
public sealed class LoadAndEnsureCustomerIsActive(FhsCommandDbContext db) : ILink<DeactivateCustomerState>
{
    public async ValueTask<LinkResult> RunAsync(DeactivateCustomerState state, CancellationToken ct)
    {
        var customer = await db.Set<Customer>()
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == state.CustomerId, ct);

        if (customer is null)
        {
            return LinkResult.Fail(CustomerErrors.NotFound(state.CustomerId));
        }

        if (!customer.IsActive)
        {
            return LinkResult.Fail(CustomerErrors.AlreadyDeactivated(state.CustomerId));
        }

        state.Customer = customer;

        return LinkResult.Continue;
    }
}
