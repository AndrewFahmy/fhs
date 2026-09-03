using System.Net;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;
using FHS.Api.Primitives;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Defects;

[Collection(nameof(FhsApiCollection))]
public sealed class GetDefectsTests(FhsApiFactory factory)
{
    private const string DefectsEndpoint = "/defects";

    [Fact]
    public async Task Lists_the_defects_for_a_station_newest_first()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var client = factory.LineOperatorClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        var raisedAt = factory.Clock.GetUtcNow();
        var first = await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "First", ct);
        var second = await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "Second", ct);
        var third = await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "Third", ct);

        var page = await DefectsHandler.GetDefectsAsync(client, $"stationCode={stationCode}", ct);

        DefectListItem Expected(Guid defectId, string description) =>
            new(
                defectId,
                stationCode,
                errorCode,
                Severity.Major,
                description,
                "Line Operator",
                raisedAt,
                null
            );

        Assert.Equal(
            [Expected(third, "Third"), Expected(second, "Second"), Expected(first, "First")],
            page.Items
        );

        Assert.Equal(
            new PageSnapshot(1, Paging.DefaultSize, TotalCount: 3, TotalPages: 1),
            PageSnapshot.From(page)
        );
    }

    [Fact]
    public async Task Pages_through_the_results_and_reports_the_total()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var client = factory.LineOperatorClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "First", ct);
        await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "Second", ct);
        await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "Third", ct);

        var firstPage = await DefectsHandler.GetDefectsAsync(
            client,
            $"stationCode={stationCode}&pageSize=2",
            ct
        );
        var secondPage = await DefectsHandler.GetDefectsAsync(
            client,
            $"stationCode={stationCode}&pageSize=2&page=2",
            ct
        );

        Assert.Equal(new PageSnapshot(1, 2, TotalCount: 3, TotalPages: 2), PageSnapshot.From(firstPage));
        Assert.Equal(new PageSnapshot(2, 2, TotalCount: 3, TotalPages: 2), PageSnapshot.From(secondPage));

        Assert.Equal(["Third", "Second"], [.. firstPage.Items.Select(i => i.Description)]);
        Assert.Equal(["First"], [.. secondPage.Items.Select(i => i.Description)]);
    }

    [Theory]
    [InlineData("pageSize=0", 1)]
    [InlineData("pageSize=999", Paging.MaxSize)]
    public async Task Clamps_the_page_size_and_echoes_the_value_it_used(string query, int expectedPageSize)
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.LineOperatorClient();
        var stationCode = client.UniqueCode("ST");

        var page = await DefectsHandler.GetDefectsAsync(client, $"stationCode={stationCode}&{query}", ct);

        Assert.Equal(
            new PageSnapshot(1, expectedPageSize, TotalCount: 0, TotalPages: 0),
            PageSnapshot.From(page)
        );
    }

    [Fact]
    public async Task Filters_by_severity()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var client = factory.LineOperatorClient();

        var stationCode = adminClient.UniqueCode("ST");
        var minorCode = adminClient.UniqueCode("EC");
        var criticalCode = adminClient.UniqueCode("EC");

        await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, minorCode, Severity.Minor, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, criticalCode, Severity.Critical, ct);

        await DefectsHandler.RaiseDefectAsync(client, stationCode, minorCode, "Cosmetic", ct);
        await DefectsHandler.RaiseDefectAsync(client, stationCode, criticalCode, "Structural", ct);

        var page = await DefectsHandler.GetDefectsAsync(
            client,
            $"stationCode={stationCode}&severity={Severity.Critical}",
            ct
        );

        Assert.Equal(["Structural"], [.. page.Items.Select(i => i.Description)]);
        Assert.Equal(criticalCode, page.Items[0].ErrorCode);
    }

    [Fact]
    public async Task Filters_by_resolution_state()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var client = factory.LineOperatorClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        await StationsHandler.CreateStationAsync(adminClient, stationCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        var resolved = await DefectsHandler.RaiseDefectAsync(
            client,
            stationCode,
            errorCode,
            "Fixed on the line",
            ct
        );
        await DefectsHandler.RaiseDefectAsync(client, stationCode, errorCode, "Still open", ct);

        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        var resolvedAt = factory.Clock.GetUtcNow();
        await DefectsHandler.ResolveDefectAsync(client, resolved, "Buffed and re-inspected", ct);

        var open = await DefectsHandler.GetDefectsAsync(
            client,
            $"stationCode={stationCode}&isResolved=false",
            ct
        );
        var closed = await DefectsHandler.GetDefectsAsync(
            client,
            $"stationCode={stationCode}&isResolved=true",
            ct
        );

        Assert.Equal(["Still open"], [.. open.Items.Select(i => i.Description)]);
        Assert.Equal(["Fixed on the line"], [.. closed.Items.Select(i => i.Description)]);
        Assert.Equal(resolvedAt, closed.Items[0].ResolvedAt);
    }

    [Fact]
    public async Task Rejects_an_invalid_filter_with_a_problem_document()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory.LineOperatorClient().GetAsync($"{DefectsEndpoint}?severity=nope", ct);

        // ThrowOnBadRequest is pinned on, so a query-string binding failure reaches
        // BadHttpRequestExceptionHandler instead of escaping as a bodiless 400 or a 500 in development.
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.BadRequest, "Request.Malformed"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_an_anonymous_request_with_401()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory.CreateAnonymousClient().GetAsync(DefectsEndpoint, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
