using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Customers.Links;

public sealed class AddCustomer(FhsCommandDbContext db) : ILink<CreateCustomerState>
{
    public ValueTask<LinkResult> RunAsync(CreateCustomerState state, CancellationToken ct)
    {
        Customer customer = new() { Code = state.Request.Code, Name = state.Request.Name };

        db.Set<Customer>().Add(customer);

        state.Produce(new CreateCustomerResponse(customer.Id));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
