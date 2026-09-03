namespace FHS.Api.Features.Customers;

public sealed record CustomerListItem(Guid CustomerId, string Code, string Name, bool IsActive);
