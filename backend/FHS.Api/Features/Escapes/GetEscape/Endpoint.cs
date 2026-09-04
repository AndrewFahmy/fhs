using FHS.Api.Extensions;
using FHS.Api.Features.Escapes.Links;
using FHS.Api.Interfaces;
using FHS.Chain;

namespace FHS.Api.Features.Escapes;

public sealed class GetEscapeEndpoint : IEndpoint
{
    private static readonly Chain<GetEscapeState, EscapeDetailResponse> Handle = ChainFactory
        .For<GetEscapeState, EscapeDetailResponse>()
        .Link<LoadEscapeDetail>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/escapes/{id:guid}",
                async (Guid id, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new GetEscapeState(id), ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Escape Details")
            .WithTags(AppConstants.Endpoints.EscapesGroupName)
            .RequireAuthorization();
    }
}
