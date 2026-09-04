using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Escapes.Links;

[Produces(nameof(CreateEscapeState.Customer))]
public sealed class LoadAndEnsureCustomerExistence(FhsCommandDbContext db) : ILink<CreateEscapeState>
{
    public async ValueTask<LinkResult> RunAsync(CreateEscapeState state, CancellationToken ct)
    {
        var customerCode = state.Request.CustomerCode;

        state.Customer = await db.Set<Customer>()
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Code == customerCode, ct);

        if (state.Customer is null)
        {
            return LinkResult.Fail(EscapeErrors.CustomerNotFound(customerCode));
        }

        if (!state.Customer.IsActive)
        {
            return LinkResult.Fail(EscapeErrors.CustomerInactive(customerCode));
        }

        return LinkResult.Continue;
    }
}
