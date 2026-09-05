using FHS.Api.Extensions;
using FHS.Api.Interfaces;
using FHS.Chain;

namespace FHS.Api.Features.Stations;

public sealed class GetStationsEndpoint : IEndpoint
{
    private static readonly Chain<GetStationsState, IReadOnlyList<StationListItem>> Handle = ChainFactory
        .For<GetStationsState, IReadOnlyList<StationListItem>>()
        .Link<QueryStation>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/stations",
                async (bool? includeInactive, ChainRunner runner, CancellationToken ct) =>
                {
                    var state = new GetStationsState(includeInactive is true);
                    var result = await runner.RunAsync(Handle, state, ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Stations")
            .WithTags(AppConstants.Endpoints.StationsGroupName)
            .RequireAuthorization()
            .Produces<IReadOnlyList<StationListItem>>(StatusCodes.Status200OK)
            .ProducesProblems(StatusCodes.Status400BadRequest);
    }
}
