using System.Net;
using System.Net.Http.Json;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Defects;

[Collection(nameof(FhsApiCollection))]
public sealed class ResolveDefectTests(FhsApiFactory factory)
{
    private static string EndpointRoute(Guid defectId) => $"/defects/{defectId}/resolve";

    [Fact]
    public async Task Marks_the_defect_resolved_and_records_the_domain_event()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.LineOperatorClient();

        var raisedAt = factory.Clock.GetUtcNow();
        var (defectId, stationId, errorCodeId) = await client.RaiseDefectAsync(factory, ct);

        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        var resolvedAt = factory.Clock.GetUtcNow();

        var response = await client.PostAsJsonAsync(
            EndpointRoute(defectId),
            new { resolution = "Buffed and re-inspected" },
            ct
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var defect = await factory.FindDefectAsync(defectId, ct);
        Assert.NotNull(defect);

        Assert.Equal(
            new DefectSnapshot(
                stationId,
                errorCodeId,
                "Scratch on door panel",
                Severity.Major,
                AppConstants.Data.LineOperatorActorId,
                CreatedAt: raisedAt,
                Resolution: "Buffed and re-inspected",
                ResolvedBy: AppConstants.Data.LineOperatorActorId,
                ResolvedAt: resolvedAt
            ),
            DefectSnapshot.From(defect)
        );

        Assert.Equal(
            [
                OutboxMessageSnapshot.For<DefectRaised>(raisedAt),
                OutboxMessageSnapshot.For<DefectResolved>(resolvedAt)
            ],
            [.. (await factory.OutboxForAsync(defectId, ct)).Select(OutboxMessageSnapshot.From)]
        );
    }

    [Fact]
    public async Task Returns_409_when_the_defect_is_already_resolved()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.LineOperatorClient();
        var (defectId, _, _) = await client.RaiseDefectAsync(factory, ct);
        var body = new { resolution = "Buffed and re-inspected" };

        await client.PostAsJsonAsync(EndpointRoute(defectId), body, ct);
        var second = await client.PostAsJsonAsync(EndpointRoute(defectId), body, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Defects.AlreadyResolved"),
            await ProblemSnapshot.FromAsync(second, ct)
        );
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_defect()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .LineOperatorClient()
            .PostAsJsonAsync(EndpointRoute(Guid.CreateVersion7()), new { resolution = "x" }, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Defects.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
