using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.Customers;

public sealed class CreateCustomerState(CreateCustomerRequest request)
    : ChainState<CreateCustomerResponse>,
        IHasRequest<CreateCustomerRequest>
{
    public CreateCustomerRequest Request { get; } = request;
}
