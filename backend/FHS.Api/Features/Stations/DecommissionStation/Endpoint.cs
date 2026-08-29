using FHS.Api.Extensions;
using FHS.Api.Features.Stations.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Stations;

public sealed class DecommissionStationEndpoint : IEndpoint
{
    private static readonly Chain<DecommissionStationState> Handle = ChainFactory
        .For<DecommissionStationState>()
        .Link<LoadAndEnsureStationIsActive>()
        .Link<MarkDecommissioned>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/stations/{id:guid}/decommission",
                async (Guid id, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new DecommissionStationState(id), ct);

                    return result.ToNoContent();
                }
            )
            .WithName("Decommission Station")
            .WithTags("Stations")
            .RequireAuthorization();
    }
}
