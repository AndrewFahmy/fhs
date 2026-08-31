using System.Net;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Stations;

[Collection(nameof(FhsApiCollection))]
public sealed class DecommissionStationTests(FhsApiFactory factory)
{
    public static string EndpointRoute(Guid stationId) => $"/stations/{stationId}/decommission";

    [Fact]
    public async Task Decommissions_once_and_then_conflicts()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var stationId = await adminClient.CreateStationAsync(adminClient.UniqueCode("ST"), ct);

        var first = await adminClient.PostAsync(EndpointRoute(stationId), null, ct);
        var second = await adminClient.PostAsync(EndpointRoute(stationId), null, ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Stations.AlreadyDecommissioned"),
            await ProblemSnapshot.FromAsync(second, ct)
        );
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_station()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory.AdminClient().PostAsync(EndpointRoute(Guid.CreateVersion7()), null, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Stations.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_a_line_operator_with_403_rather_than_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var stationId = await adminClient.CreateStationAsync(adminClient.UniqueCode("ST"), ct);

        var response = await factory.LineOperatorClient().PostAsync(EndpointRoute(stationId), null, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
