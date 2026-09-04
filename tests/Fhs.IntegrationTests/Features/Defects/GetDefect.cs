using System.Net;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Defects;

[Collection(nameof(FhsApiCollection))]
public sealed class GetDefectTests(FhsApiFactory factory)
{
    [Fact]
    public async Task Returns_the_whole_detail_for_an_open_defect()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var client = factory.CreateLineOperatorClient();

        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        // Name and description differ from their codes on purpose: if the projection reached for the
        // wrong column of the joined row, equal values would hide it.
        var stationId = await StationsHandler.CreateStationAsync(
            adminClient,
            stationCode,
            ct,
            "Door line, bay 4"
        );
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            errorCode,
            Severity.Critical,
            ct,
            "Paint runs"
        );

        var raisedAt = factory.Clock.GetUtcNow();
        var defectId = await DefectsHandler.RaiseDefectAsync(
            client,
            stationCode,
            errorCode,
            "Scratch on door panel",
            ct
        );

        Assert.Equal(
            new DefectDetailResponse(
                defectId,
                stationId,
                stationCode,
                "Door line, bay 4",
                errorCodeId,
                errorCode,
                "Paint runs",
                Severity.Critical,
                "Scratch on door panel",
                RaisedBy: "Line Operator",
                CreatedAt: raisedAt,
                Resolution: null,
                ResolvedBy: null,
                ResolvedAt: null
            ),
            await DefectsHandler.GetDefectAsync(client, defectId, ct)
        );
    }

    [Fact]
    public async Task Reports_the_resolver_once_the_defect_is_resolved()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateLineOperatorClient();
        var (defectId, _, _) = await DefectsHandler.RaiseDefectAsync(client, factory, ct);

        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        var resolvedAt = factory.Clock.GetUtcNow();

        await DefectsHandler.ResolveDefectAsync(client, defectId, "Buffed and re-inspected", ct);

        var detail = await DefectsHandler.GetDefectAsync(client, defectId, ct);

        // Resolver is the one optional navigation, so this is the only test that exercises its LEFT JOIN
        // with a row on the other side — the test above covers the null case.
        Assert.Equal("Buffed and re-inspected", detail.Resolution);
        Assert.Equal("Line Operator", detail.ResolvedBy);
        Assert.Equal(resolvedAt, detail.ResolvedAt);
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_defect()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory.CreateLineOperatorClient().GetAsync($"/defects/{Guid.CreateVersion7()}", ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Defects.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
