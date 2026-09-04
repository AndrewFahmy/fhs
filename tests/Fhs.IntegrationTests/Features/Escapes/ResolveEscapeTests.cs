using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using FHS.Api.Features.Escapes;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Escapes;

[Collection(nameof(FhsApiCollection))]
public sealed class ResolveEscapeTests(FhsApiFactory factory)
{
    private static string EndpointRoute(Guid escapeId) => $"/escapes/{escapeId}/resolve";

    [Fact]
    public async Task Marks_the_escape_resolved_and_records_the_domain_event()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateLineOperatorClient();

        var reportedAt = factory.Clock.GetUtcNow();
        var (escapeId, customerId, errorCodeId) = await EscapesHandler.ReportEscapeAsync(client, factory, ct);

        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        var resolvedAt = factory.Clock.GetUtcNow();

        var response = await client.PostAsJsonAsync(
            EndpointRoute(escapeId),
            new { resolution = "Replacement shipped and root cause contained" },
            ct
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var escape = await factory.FindAsync<Escape>(escapeId, ct);
        Assert.NotNull(escape);

        Assert.Equal(
            new EscapeSnapshot(
                customerId,
                errorCodeId,
                "Paint run found at customer",
                Severity.Major,
                AppConstants.Data.LineOperatorActorId,
                ReportedAt: reportedAt,
                Resolution: "Replacement shipped and root cause contained",
                ResolvedBy: AppConstants.Data.LineOperatorActorId,
                ResolvedAt: resolvedAt
            ),
            EscapeSnapshot.From(escape)
        );

        Assert.Equal(
            [
                OutboxMessageSnapshot.For(
                    new EscapeReported(escapeId, customerId, errorCodeId, Severity.Major, reportedAt)
                ),
                OutboxMessageSnapshot.For(
                    new EscapeResolved(escapeId, AppConstants.Data.LineOperatorActorId, resolvedAt)
                )
            ],
            [.. (await factory.OutboxForAsync(escapeId, ct)).Select(OutboxMessageSnapshot.From)]
        );
    }

    [Fact]
    public async Task Returns_409_when_the_escape_is_already_resolved()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateLineOperatorClient();
        var (escapeId, _, _) = await EscapesHandler.ReportEscapeAsync(client, factory, ct);
        var body = new { resolution = "Replacement shipped" };

        await client.PostAsJsonAsync(EndpointRoute(escapeId), body, ct);
        var second = await client.PostAsJsonAsync(EndpointRoute(escapeId), body, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Escapes.AlreadyResolved"),
            await ProblemSnapshot.FromAsync(second, ct)
        );
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_escape()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(EndpointRoute(Guid.CreateVersion7()), new { resolution = "x" }, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Escapes.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
