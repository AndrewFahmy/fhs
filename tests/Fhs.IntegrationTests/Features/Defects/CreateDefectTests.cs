using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Defects;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateDefectTests(FhsApiFactory factory)
{
    private const string DefectsEndpoint = "/defects";

    [Fact]
    public async Task Persists_the_defect_and_records_the_domain_event()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        var stationId = await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            errorCode,
            Severity.Critical,
            ct
        );
        var raisedAt = factory.Clock.GetUtcNow();

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                DefectsEndpoint,
                new
                {
                    stationCode,
                    errorCode,
                    description = "Scratch on door panel"
                },
                ct
            );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CreateDefectResponse>(ct);
        Assert.NotNull(created);
        Assert.Equal($"/defects/{created.DefectId}", response.Headers.Location?.ToString());

        var defect = await factory.FindAsync<Defect>(created.DefectId, ct);
        Assert.NotNull(defect);

        Assert.Equal(
            new DefectSnapshot(
                stationId,
                errorCodeId,
                "Scratch on door panel",
                Severity.Critical,
                AppConstants.Data.LineOperatorActorId,
                CreatedAt: raisedAt,
                Resolution: null,
                ResolvedBy: null,
                ResolvedAt: null
            ),
            DefectSnapshot.From(defect)
        );

        var messages = await factory.OutboxForAsync(created.DefectId, ct);
        Assert.Equal(
            [
                OutboxMessageSnapshot.For(
                    new DefectRaised(created.DefectId, stationId, errorCodeId, Severity.Critical, raisedAt)
                )
            ],
            [.. messages.Select(OutboxMessageSnapshot.From)]
        );

        // Serialized against the runtime type rather than IDomainEvent — the whole event, not just OccurredAt.
        Assert.Contains(nameof(DefectRaised.StationId).ToCamelCase(), messages[0].Payload);
        Assert.Contains(nameof(Severity.Critical), messages[0].Payload);
    }

    [Fact]
    public async Task Reports_every_shape_failure_at_once()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                DefectsEndpoint,
                new
                {
                    stationCode = "",
                    errorCode = "",
                    description = ""
                },
                ct
            );

        Assert.Equal(
            new ValidationProblemSnapshot(HttpStatusCode.BadRequest, "Validation.Failed", ErrorCount: 3),
            await ValidationProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Returns_404_when_the_station_does_not_exist()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var errorCode = adminClient.UniqueCode("EC");

        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                DefectsEndpoint,
                new
                {
                    stationCode = "NO-SUCH-STATION",
                    errorCode,
                    description = "x"
                },
                ct
            );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Defects.StationNotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Returns_409_when_the_station_is_decommissioned()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        var stationId = await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);
        await StationsHandler.DecommissionStationAsync(adminClient, stationId, ct);

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                DefectsEndpoint,
                new
                {
                    stationCode,
                    errorCode,
                    description = "x"
                },
                ct
            );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Defects.StationInactive"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Returns_409_when_the_error_code_is_retired()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            errorCode,
            Severity.Major,
            ct
        );
        await ErrorCodesHandler.RetireErrorCodeAsync(adminClient, errorCodeId, ct);

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                DefectsEndpoint,
                new
                {
                    stationCode,
                    errorCode,
                    description = "x"
                },
                ct
            );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Defects.ErrorCodeInactive"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
