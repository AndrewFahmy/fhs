using FHS.Api.Extensions;
using FHS.Api.Features.Customers.Links;
using FHS.Api.Interfaces;
using FHS.Chain;

namespace FHS.Api.Features.Customers;

public sealed class GetCustomersEndpoint : IEndpoint
{
    private static readonly Chain<GetCustomerState, IReadOnlyList<CustomerListItem>> Handle = ChainFactory
        .For<GetCustomerState, IReadOnlyList<CustomerListItem>>()
        .Link<QueryCustomers>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/customers",
                async (bool? includeInactive, ChainRunner runner, CancellationToken ct) =>
                {
                    var state = new GetCustomerState(includeInactive is true);
                    var result = await runner.RunAsync(Handle, state, ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Customers")
            .WithTags(AppConstants.Endpoints.CustomersGroupName)
            .RequireAuthorization();
    }
}
