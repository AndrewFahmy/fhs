using FHS.Api.Extensions;
using FHS.Api.Features.ErrorCodes.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.ErrorCodes;

public sealed class RetireErrorCodeEndpoint : IEndpoint
{
    private static readonly Chain<RetireErrorCodeState> Handle = ChainFactory
        .For<RetireErrorCodeState>()
        .Link<ResolveActor>()
        .Link<LoadAndEnsureErrorCodeIsActive>()
        .Link<MarkErrorCodeAsRetired>()
        .Link<RecordDomainEvents>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/error-codes/{id:guid}/retire",
                async (Guid id, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new RetireErrorCodeState(id), ct);

                    return result.ToNoContent();
                }
            )
            .WithName("Retire Error Code")
            .WithTags(AppConstants.Endpoints.ErrorCodesGroupName)
            .RequireAuthorization(AppConstants.Auth.AdminAccessPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblems(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);
    }
}
