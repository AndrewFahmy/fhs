using System.Net;
using FHS.Api.Enums;
using Fhs.IntegrationTests.Extensions;
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
        var errorCodeId = await adminClient.CreateErrorCodeAsync(
            adminClient.UniqueCode("EC"),
            Severity.Major,
            ct
        );

        var first = await adminClient.PostAsync(EndpointRoute(errorCodeId), null, ct);
        var second = await adminClient.PostAsync(EndpointRoute(errorCodeId), null, ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
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
        var errorCodeId = await adminClient.CreateErrorCodeAsync(
            adminClient.UniqueCode("EC"),
            Severity.Major,
            ct
        );

        var response = await factory.LineOperatorClient().PostAsync(EndpointRoute(errorCodeId), null, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
