using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using FHS.IntegrationTests.Extensions;
using FHS.IntegrationTests.Handlers;
using FHS.IntegrationTests.Snapshots;

namespace FHS.IntegrationTests.Features.Stations;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateStationTests(FhsApiFactory factory)
{
    private const string CreateStationEndpoint = "/stations";

    [Fact]
    public async Task Persists_the_station_and_rejects_a_duplicate_code()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("ST");
        var name = $"{code} assembly bay";
        var facilityCode = AppConstants.Data.DefaultFacilityCode;

        var stationId = await StationsHandler.CreateStationAsync(adminClient, code, facilityCode, ct, name);

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
            .CreateAdminClient()
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
        var client = factory.CreateLineOperatorClient();
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

    [Fact]
    public async Task Binds_the_station_to_the_facility_it_names()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var facilityCode = adminClient.UniqueCode("FAC");
        var code = adminClient.UniqueCode("ST");

        var facilityId = await FacilitiesHandler.CreateFacilityAsync(adminClient, facilityCode, ct);
        var stationId = await StationsHandler.CreateStationAsync(adminClient, code, facilityCode, ct);

        var station = await factory.FindAsync<Station>(stationId, ct);

        Assert.NotNull(station);
        Assert.Equal(facilityId, station.FacilityId);
    }

    [Fact]
    public async Task Rejects_a_station_for_an_unknown_facility()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("ST");
        var facilityCode = adminClient.UniqueCode("FAC");

        var response = await adminClient.PostAsJsonAsync(
            CreateStationEndpoint,
            new
            {
                code,
                facilityCode,
                name = code
            },
            ct
        );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Stations.FacilityNotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
