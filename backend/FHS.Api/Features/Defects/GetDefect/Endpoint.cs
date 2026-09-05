using FHS.Api.Extensions;
using FHS.Api.Features.Defects.Links;
using FHS.Api.Interfaces;
using FHS.Chain;

namespace FHS.Api.Features.Defects;

public sealed class GetDefectEndpoint : IEndpoint
{
    private static readonly Chain<GetDefectState, DefectDetailResponse> Handle = ChainFactory
        .For<GetDefectState, DefectDetailResponse>()
        .Link<LoadDefectDetail>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/defects/{id:guid}",
                async (Guid id, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new GetDefectState(id), ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Defect Details")
            .WithTags(AppConstants.Endpoints.DefectsGroupName)
            .RequireAuthorization()
            .Produces<DefectDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
