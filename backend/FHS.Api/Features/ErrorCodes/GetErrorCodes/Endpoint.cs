using FHS.Api.Extensions;
using FHS.Api.Features.ErrorCodes.Links;
using FHS.Api.Interfaces;
using FHS.Chain;

namespace FHS.Api.Features.ErrorCodes;

public sealed class GetErrorCodesEndpoint : IEndpoint
{
    private static readonly Chain<GetErrorCodesState, IReadOnlyList<ErrorCodeListItem>> Handle = ChainFactory
        .For<GetErrorCodesState, IReadOnlyList<ErrorCodeListItem>>()
        .Link<QueryErrorCodes>()
        .Build();

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/error-codes",
                async (bool? includeInactive, ChainRunner runner, CancellationToken ct) =>
                {
                    var state = new GetErrorCodesState(includeInactive is true);
                    var result = await runner.RunAsync(Handle, state, ct);

                    return result.ToOk();
                }
            )
            .WithName("Get Error Codes")
            .WithTags(AppConstants.Endpoints.ErrorCodesGroupName)
            .RequireAuthorization()
            .Produces<IReadOnlyList<ErrorCodeListItem>>(StatusCodes.Status200OK)
            .ProducesProblems(StatusCodes.Status400BadRequest);
    }
}
