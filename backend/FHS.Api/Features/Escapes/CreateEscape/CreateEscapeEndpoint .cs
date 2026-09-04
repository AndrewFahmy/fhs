using FHS.Api.Extensions;
using FHS.Api.Features.Escapes;
using FHS.Api.Features.Escapes.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

public sealed class CreateEscapeEndpoint : IEndpoint
{
    private static readonly Chain<CreateEscapeState, CreateEscapeResponse> Handle = ChainFactory
        .For<CreateEscapeState, CreateEscapeResponse>()
        .Link<ValidateRequestInput<CreateEscapeRequest>>()
        .Link<ResolveActor>()
        .Link<LoadAndEnsureCustomerExistence>()
        .Link<ClassifyEscape>()
        .Link<ReportEscape>()
        .Link<RecordDomainEvents>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/escapes",
                async (CreateEscapeRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new CreateEscapeState(request), ct);

                    return result.ToCreated(r => $"/escapes/{r.EscapeId}");
                }
            )
            .WithName("Create Escape")
            .WithTags(AppConstants.Endpoints.EscapesGroupName)
            .RequireAuthorization();
    }
}
