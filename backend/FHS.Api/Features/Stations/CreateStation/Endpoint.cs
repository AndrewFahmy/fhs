using FHS.Api.Extensions;
using FHS.Api.Features.Stations.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Stations;

public sealed class CreateStationEndpoint : IEndpoint
{
    private static readonly Chain<CreateStationState, CreateStationResponse> Handle = ChainFactory
        .For<CreateStationState, CreateStationResponse>()
        .Link<ValidateRequestInput<CreateStationRequest>>()
        .Link<EnsureStationCodeIsUnique>()
        .Link<AddStation>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/stations",
                async (CreateStationRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new CreateStationState(request), ct);

                    return result.ToCreated(r => $"/stations/{r.StationId}");
                }
            )
            .WithName("Create Station")
            .WithTags(AppConstants.Endpoints.StationsGroupName)
            .RequireAuthorization(AppConstants.Auth.AdminAccessPolicy);
    }
}
