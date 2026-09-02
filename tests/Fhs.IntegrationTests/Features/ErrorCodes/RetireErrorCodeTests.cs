using System.Net;
using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.ErrorCodes;

[Collection(nameof(FhsApiCollection))]
public sealed class RetireErrorCodeTests(FhsApiFactory factory)
{
    private static string EndpointRoute(Guid errorCodeId) => $"/error-codes/{errorCodeId}/retire";

    [Fact]
    public async Task Retires_once_and_then_conflicts()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var code = adminClient.UniqueCode("EC");
        var description = "Paint runs on the outer skin";

        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            code,
            Severity.Major,
            ct,
            description
        );

        var first = await adminClient.PostAsync(EndpointRoute(errorCodeId), null, ct);
        var second = await adminClient.PostAsync(EndpointRoute(errorCodeId), null, ct);

        var errorCode = await factory.FindAsync<ErrorCode>(errorCodeId, ct);
        Assert.NotNull(errorCode);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        Assert.Equal(
            new ErrorCodeSnapshot(code, description, Severity.Major, IsActive: false),
            ErrorCodeSnapshot.From(errorCode)
        );
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "ErrorCodes.AlreadyRetired"),
            await ProblemSnapshot.FromAsync(second, ct)
        );
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_error_code()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory.AdminClient().PostAsync(EndpointRoute(Guid.CreateVersion7()), null, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "ErrorCodes.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_a_line_operator_with_403_rather_than_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            adminClient.UniqueCode("EC"),
            Severity.Major,
            ct
        );

        var response = await factory.LineOperatorClient().PostAsync(EndpointRoute(errorCodeId), null, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
