using FHS.Api.Extensions;
using FHS.Api.Features.Customers.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Customers;

public sealed class DeactivateCustomerEndpoint : IEndpoint
{
    private static readonly Chain<DeactivateCustomerState> Handle = ChainFactory
        .For<DeactivateCustomerState>()
        .Link<ResolveActor>()
        .Link<LoadAndEnsureCustomerIsActive>()
        .Link<MarkCustomerAsDeactivated>()
        .Link<RecordDomainEvents>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/customers/{id:guid}/deactivate",
                async (Guid id, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new DeactivateCustomerState(id), ct);

                    return result.ToNoContent();
                }
            )
            .WithName("Deactivate Customer")
            .WithTags(AppConstants.Endpoints.CustomersGroupName)
            .RequireAuthorization(AppConstants.Auth.AdminAccessPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblems(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);
    }
}
