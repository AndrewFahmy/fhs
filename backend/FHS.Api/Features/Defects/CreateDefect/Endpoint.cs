using FHS.Api.Extensions;
using FHS.Api.Features.Defects.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Defects;

public sealed class CreateDefectEndpoint : IEndpoint
{
    private static readonly Chain<CreateDefectState, CreateDefectResponse> Handle = ChainFactory
        .For<CreateDefectState, CreateDefectResponse>()
        .Link<ValidateRequestInput<CreateDefectRequest>>()
        .Link<ResolveActor>()
        .Link<LoadAndEnsureStationExistence>()
        .Link<ClassifyDefect>()
        .Link<RaiseDefect>()
        .Link<RecordDomainEvents>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/defects",
                async (CreateDefectRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new CreateDefectState(request), ct);

                    return result.ToCreated(r => $"/defects/{r.DefectId}");
                }
            )
            .WithName("Create Defect")
            .WithTags(AppConstants.Endpoints.DefectsGroupName)
            .RequireAuthorization()
            .Produces<CreateDefectResponse>(StatusCodes.Status201Created)
            .ProducesProblems(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict
            );
    }
}
