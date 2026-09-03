using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Customers.Links;

public sealed class QueryCustomers(FhsQueryDbContext db) : ILink<GetCustomerState>
{
    public async ValueTask<LinkResult> RunAsync(GetCustomerState state, CancellationToken ct)
    {
        IQueryable<Customer> customers = db.Set<Customer>();

        if (!state.IncludeInactive)
        {
            customers = customers.Where(c => c.IsActive);
        }

        var items = await customers
            .OrderBy(c => c.Code)
            .Select(c => new CustomerListItem(c.Id, c.Code, c.Name, c.IsActive))
            .ToListAsync(ct);

        state.Produce(items);

        return LinkResult.Continue;
    }
}
