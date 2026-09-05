using FHS.Api.Extensions;
using FHS.Api.Features.Escapes.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.Escapes;

public sealed class ResolveEscapeEndpoint : IEndpoint
{
    private static readonly Chain<ResolveEscapeState> Handle = ChainFactory
        .For<ResolveEscapeState>()
        .Link<ValidateRequestInput<ResolveEscapeRequest>>()
        .Link<ResolveActor>()
        .Link<LoadAndEnsureEscapeIsOpen>()
        .Link<ResolveEscape>()
        .Link<RecordDomainEvents>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/escapes/{id:guid}/resolve",
                async (Guid id, ResolveEscapeRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new ResolveEscapeState(id, request), ct);

                    return result.ToNoContent();
                }
            )
            .WithName("Resolve Escape")
            .WithTags(AppConstants.Endpoints.EscapesGroupName)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblems(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict
            );
    }
}
