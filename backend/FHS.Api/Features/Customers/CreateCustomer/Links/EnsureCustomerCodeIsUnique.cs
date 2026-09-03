using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Customers.Links;

public sealed class EnsureCustomerCodeIsUnique(FhsCommandDbContext db) : ILink<CreateCustomerState>
{
    public async ValueTask<LinkResult> RunAsync(CreateCustomerState state, CancellationToken ct)
    {
        var code = state.Request.Code;
        var exists = await db.Set<Customer>().AsNoTracking().AnyAsync(c => c.Code == code, ct);

        return exists ? LinkResult.Fail(CustomerErrors.CodeAlreadyExists(code)) : LinkResult.Continue;
    }
}
