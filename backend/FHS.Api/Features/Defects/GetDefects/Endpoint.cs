using FHS.Api.Extensions;
using FHS.Api.Features.Defects.Links;
using FHS.Api.Interfaces;
using FHS.Api.Primitives;
using FHS.Chain;

namespace FHS.Api.Features.Defects;

public sealed class GetDefectsEndpoint : IEndpoint
{
    private static readonly Chain<GetDefectsState, PagedResponse<DefectListItem>> Handle = ChainFactory
        .For<GetDefectsState, PagedResponse<DefectListItem>>()
        .Link<QueryDefects>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/defects",
                async ([AsParameters] GetDefectsRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new GetDefectsState(request), ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Defects")
            .WithTags(AppConstants.Endpoints.DefectsGroupName)
            .RequireAuthorization();
    }
}
