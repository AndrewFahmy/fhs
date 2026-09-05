using FHS.Api.Extensions;
using FHS.Api.Features.ErrorCodes.Links;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Chain;

namespace FHS.Api.Features.ErrorCodes;

public sealed class CreateErrorCodeEndpoint : IEndpoint
{
    private static readonly Chain<CreateErrorCodeState, CreateErrorCodeResponse> Handle = ChainFactory
        .For<CreateErrorCodeState, CreateErrorCodeResponse>()
        .Link<ValidateRequestInput<CreateErrorCodeRequest>>()
        .Link<EnsureErrorCodeIsUnique>()
        .Link<AddErrorCode>()
        .Link<SaveChanges>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/error-codes",
                async (CreateErrorCodeRequest request, ChainRunner runner, CancellationToken ct) =>
                {
                    var result = await runner.RunAsync(Handle, new CreateErrorCodeState(request), ct);

                    return result.ToCreated(r => $"/error-codes/{r.ErrorCodeId}");
                }
            )
            .WithName("Create Error Code")
            .WithTags(AppConstants.Endpoints.ErrorCodesGroupName)
            .RequireAuthorization(AppConstants.Auth.AdminAccessPolicy)
            .Produces<CreateErrorCodeResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblems(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);
    }
}
