using FHS.Chain.Contracts;

namespace FHS.Api.Features.Customers;

public sealed class GetCustomerState(bool includeInactive) : ChainState<IReadOnlyList<CustomerListItem>>
{
    public bool IncludeInactive { get; } = includeInactive;
}
