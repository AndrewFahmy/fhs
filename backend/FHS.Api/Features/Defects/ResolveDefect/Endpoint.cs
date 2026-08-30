using FHS.Api.Extensions;
using FHS.Api.Features.Defects.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Defects;

public sealed class ResolveDefectEndpoint : IEndpoint
{
    private static readonly Chain<ResolveDefectState> Handle = ChainFactory
        .For<ResolveDefectState>()
        .Link<ValidateRequestInput<ResolveDefectRequest>>()
        .Link<ResolveActor>()
        .Link<LoadAndEnsureDefectIsOpen>()
        .Link<MarkResolved>()
        .Link<RecordDomainEvents>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/defects/{id:guid}/resolve",
                async (Guid id, ResolveDefectRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new ResolveDefectState(id, request), ct);

                    return result.ToNoContent();
                }
            )
            .WithName("Resolve Defect")
            .WithTags(AppConstants.Endpoints.DefectsGroupName)
            .RequireAuthorization();
    }
}
