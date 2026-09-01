using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Stations;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateStationTests(FhsApiFactory factory)
{
    private const string CreateStationEndpoint = "/stations";

    [Fact]
    public async Task Persists_the_station_and_rejects_a_duplicate_code()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var code = adminClient.UniqueCode("ST");
        var name = $"{code} assembly bay";

        var stationId = await adminClient.CreateStationAsync(code, ct, name);

        // The 409 is the assertion: only a persisted row can collide.
        var duplicate = await adminClient.PostAsJsonAsync(CreateStationEndpoint, new { code, name }, ct);

        var station = await factory.FindAsync<Station>(stationId, ct);
        Assert.NotNull(station);

        Assert.Equal(new StationSnapshot(code, name, IsActive: true), StationSnapshot.From(station));
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Stations.CodeAlreadyExists"),
            await ProblemSnapshot.FromAsync(duplicate, ct)
        );
    }

    [Fact]
    public async Task Reports_every_shape_failure_at_once()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .AdminClient()
            .PostAsJsonAsync(CreateStationEndpoint, new { code = "", name = "" }, ct);

        Assert.Equal(
            new ValidationProblemSnapshot(HttpStatusCode.BadRequest, "Validation.Failed", ErrorCount: 2),
            await ValidationProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_a_line_operator_with_403_rather_than_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.LineOperatorClient();
        var code = client.UniqueCode("ST");

        var response = await client.PostAsJsonAsync(CreateStationEndpoint, new { code, name = code }, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refuses_an_anonymous_request_with_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateAnonymousClient();
        var code = client.UniqueCode("ST");

        var response = await client.PostAsJsonAsync(CreateStationEndpoint, new { code, name = code }, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
