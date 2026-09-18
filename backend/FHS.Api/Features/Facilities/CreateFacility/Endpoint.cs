using FHS.Api.Extensions;
using FHS.Api.Features.Facilities.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Facilities;

public sealed class CreateFacilityEndpoint : IEndpoint
{
    private static readonly Chain<CreateFacilityState, CreateFacilityResponse> Handle = ChainFactory
        .For<CreateFacilityState, CreateFacilityResponse>()
        .Link<ValidateRequestInput<CreateFacilityRequest>>()
        .Link<EnsureFacilityCodeIsUnique>()
        .Link<AddFacility>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/facilities",
                async (CreateFacilityRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new CreateFacilityState(request), ct);

                    return result.ToCreated(r => $"/facilities/{r.FacilityId}");
                }
            )
            .WithName("Create Facility")
            .WithTags(AppConstants.Endpoints.FacilitiesGroupName)
            .RequireAuthorization(AppConstants.Auth.AdminAccessPolicy)
            .Produces<CreateFacilityResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblems(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);
    }
}
