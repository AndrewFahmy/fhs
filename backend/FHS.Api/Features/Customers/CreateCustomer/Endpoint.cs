using FHS.Api.Extensions;
using FHS.Api.Features.Customers.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Customers;

public sealed class CreateCustomerEndpoint : IEndpoint
{
    private static readonly Chain<CreateCustomerState, CreateCustomerResponse> Handle = ChainFactory
        .For<CreateCustomerState, CreateCustomerResponse>()
        .Link<ValidateRequestInput<CreateCustomerRequest>>()
        .Link<EnsureCustomerCodeIsUnique>()
        .Link<AddCustomer>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/customers",
                async (CreateCustomerRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new CreateCustomerState(request), ct);

                    return result.ToCreated(r => $"/customers/{r.CustomerId}");
                }
            )
            .WithName("Create Customer")
            .WithTags(AppConstants.Endpoints.CustomersGroupName)
            .RequireAuthorization(AppConstants.Auth.AdminAccessPolicy);
    }
}
