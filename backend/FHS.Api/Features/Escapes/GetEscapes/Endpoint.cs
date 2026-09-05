using FHS.Api.Extensions;
using FHS.Api.Interfaces;
using FHS.Api.Primitives;
using FHS.Chain;

namespace FHS.Api.Features.Escapes;

public sealed class GetEscapesEndpoint : IEndpoint
{
    private static readonly Chain<GetEscapesState, PagedResponse<EscapeListItem>> Handle = ChainFactory
        .For<GetEscapesState, PagedResponse<EscapeListItem>>()
        .Link<QueryEscapes>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/escapes",
                async ([AsParameters] GetEscapesRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new GetEscapesState(request), ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Escapes")
            .WithTags(AppConstants.Endpoints.EscapesGroupName)
            .RequireAuthorization()
            .Produces<PagedResponse<EscapeListItem>>(StatusCodes.Status200OK)
            .ProducesProblems(StatusCodes.Status400BadRequest);
    }
}
