namespace FHS.Api.Features.Customers;

public sealed record CreateCustomerRequest(string Code, string Name);

public sealed record CreateCustomerResponse(Guid CustomerId);
