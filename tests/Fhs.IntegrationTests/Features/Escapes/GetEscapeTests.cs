using System.Net;
using FHS.Api.Enums;
using FHS.Api.Features.Escapes;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Escapes;

[Collection(nameof(FhsApiCollection))]
public sealed class GetEscapeTests(FhsApiFactory factory)
{
    [Fact]
    public async Task Returns_the_whole_detail_for_an_open_escape()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var client = factory.CreateLineOperatorClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        // Name and description differ from their codes on purpose: if the projection reached for the
        // wrong column of the joined row, equal values would hide it.
        var customerId = await CustomersHandler.CreateCustomerAsync(
            adminClient,
            customerCode,
            ct,
            "Northfield Motors"
        );
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            errorCode,
            Severity.Critical,
            ct,
            "Paint runs"
        );

        var reportedAt = factory.Clock.GetUtcNow();
        var escapeId = await EscapesHandler.ReportEscapeAsync(
            client,
            customerCode,
            errorCode,
            "Paint run found at customer",
            ct
        );

        Assert.Equal(
            new EscapeDetailResponse(
                escapeId,
                customerId,
                customerCode,
                "Northfield Motors",
                errorCodeId,
                errorCode,
                "Paint runs",
                Severity.Critical,
                "Paint run found at customer",
                ReportedBy: "Line Operator",
                ReportedAt: reportedAt,
                Resolution: null,
                ResolvedBy: null,
                ResolvedAt: null
            ),
            await EscapesHandler.GetEscapeAsync(client, escapeId, ct)
        );
    }

    [Fact]
    public async Task Reports_the_resolver_once_the_escape_is_resolved()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateLineOperatorClient();
        var (escapeId, _, _) = await EscapesHandler.ReportEscapeAsync(client, factory, ct);

        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        var resolvedAt = factory.Clock.GetUtcNow();

        await EscapesHandler.ResolveEscapeAsync(client, escapeId, "Replacement shipped", ct);

        var detail = await EscapesHandler.GetEscapeAsync(client, escapeId, ct);

        // Resolver is the one optional navigation, so this is the only test that exercises its LEFT JOIN
        // with a row on the other side — the test above covers the null case.
        Assert.Equal("Replacement shipped", detail.Resolution);
        Assert.Equal("Line Operator", detail.ResolvedBy);
        Assert.Equal(resolvedAt, detail.ResolvedAt);
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_escape()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateLineOperatorClient()
            .GetAsync($"/escapes/{Guid.CreateVersion7()}", ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Escapes.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
