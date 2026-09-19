using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using FHS.IntegrationTests.Extensions;
using FHS.IntegrationTests.Handlers;
using FHS.IntegrationTests.Snapshots;

namespace FHS.IntegrationTests.Features.Facilities;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateFacilityTests(FhsApiFactory factory)
{
    private const string CreateFacilityEndpoint = "/facilities";

    [Fact]
    public async Task Persists_the_facility_and_rejects_a_duplicate_code()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("FAC");
        var name = $"{code} works";

        var facilityId = await FacilitiesHandler.CreateFacilityAsync(adminClient, code, ct, name);

        // The 409 is the assertion: only a persisted row can collide.
        var duplicate = await adminClient.PostAsJsonAsync(CreateFacilityEndpoint, new { code, name }, ct);

        var facility = await factory.FindAsync<Facility>(facilityId, ct);
        Assert.NotNull(facility);

        Assert.Equal(new FacilitySnapshot(code, name, IsActive: true), FacilitySnapshot.From(facility));
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Facilities.CodeAlreadyExists"),
            await ProblemSnapshot.FromAsync(duplicate, ct)
        );
    }

    [Fact]
    public async Task Reports_every_shape_failure_at_once()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateAdminClient()
            .PostAsJsonAsync(CreateFacilityEndpoint, new { code = "", name = "" }, ct);

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
        var code = client.UniqueCode("FAC");

        var response = await client.PostAsJsonAsync(CreateFacilityEndpoint, new { code, name = code }, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refuses_an_anonymous_request_with_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateAnonymousClient();
        var code = client.UniqueCode("FAC");

        var response = await client.PostAsJsonAsync(CreateFacilityEndpoint, new { code, name = code }, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
