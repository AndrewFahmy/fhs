using System.Net;
using FHS.Api.Data.Entities;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
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
        var code = adminClient.UniqueCode("ST");
        var name = $"{code} assembly bay";

        var stationId = await StationsHandler.CreateStationAsync(adminClient, code, ct, name);

        var first = await adminClient.PostAsync(EndpointRoute(stationId), null, ct);
        var second = await adminClient.PostAsync(EndpointRoute(stationId), null, ct);

        var station = await factory.FindAsync<Station>(stationId, ct);
        Assert.NotNull(station);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(new StationSnapshot(code, name, IsActive: false), StationSnapshot.From(station));
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
        var stationId = await StationsHandler.CreateStationAsync(
            adminClient,
            adminClient.UniqueCode("ST"),
            ct
        );

        var response = await factory.LineOperatorClient().PostAsync(EndpointRoute(stationId), null, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
