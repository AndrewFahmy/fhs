using System.Net;
using FHS.Api.Enums;
using FHS.Api.Primitives;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;
using FHS.Api.Features.Escapes;

namespace Fhs.IntegrationTests.Features.Escapes;

[Collection(nameof(FhsApiCollection))]
public sealed class GetEscapesTests(FhsApiFactory factory)
{
    private const string EscapesEndpoint = "/escapes";

    [Fact]
    public async Task Lists_the_escapes_for_a_customer_newest_first()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var client = factory.CreateLineOperatorClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        var reportedAt = factory.Clock.GetUtcNow();
        var first = await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "First", ct);
        var second = await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "Second", ct);
        var third = await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "Third", ct);

        var page = await EscapesHandler.GetEscapesAsync(client, $"customerCode={customerCode}", ct);

        EscapeListItem Expected(Guid escapeId, string description) =>
            new(
                escapeId,
                customerCode,
                errorCode,
                Severity.Major,
                description,
                "Line Operator",
                reportedAt,
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
        var adminClient = factory.CreateAdminClient();
        var client = factory.CreateLineOperatorClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "First", ct);
        await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "Second", ct);
        await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "Third", ct);

        var firstPage = await EscapesHandler.GetEscapesAsync(
            client,
            $"customerCode={customerCode}&pageSize=2",
            ct
        );
        var secondPage = await EscapesHandler.GetEscapesAsync(
            client,
            $"customerCode={customerCode}&pageSize=2&page=2",
            ct
        );

        Assert.Equal(new PageSnapshot(1, 2, TotalCount: 3, TotalPages: 2), PageSnapshot.From(firstPage));
        Assert.Equal(new PageSnapshot(2, 2, TotalCount: 3, TotalPages: 2), PageSnapshot.From(secondPage));

        Assert.Equal(["Third", "Second"], [.. firstPage.Items.Select(i => i.Description)]);
        Assert.Equal(["First"], [.. secondPage.Items.Select(i => i.Description)]);
    }

    [Fact]
    public async Task Filters_by_severity()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var client = factory.CreateLineOperatorClient();

        var customerCode = adminClient.UniqueCode("CU");
        var minorCode = adminClient.UniqueCode("EC");
        var criticalCode = adminClient.UniqueCode("EC");

        await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, minorCode, Severity.Minor, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, criticalCode, Severity.Critical, ct);

        await EscapesHandler.ReportEscapeAsync(client, customerCode, minorCode, "Cosmetic", ct);
        await EscapesHandler.ReportEscapeAsync(client, customerCode, criticalCode, "Structural", ct);

        var page = await EscapesHandler.GetEscapesAsync(
            client,
            $"customerCode={customerCode}&severity={Severity.Critical}",
            ct
        );

        Assert.Equal(["Structural"], [.. page.Items.Select(i => i.Description)]);
        Assert.Equal(criticalCode, page.Items[0].ErrorCode);
    }

    [Fact]
    public async Task Filters_by_resolution_state()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var client = factory.CreateLineOperatorClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        var resolved = await EscapesHandler.ReportEscapeAsync(
            client,
            customerCode,
            errorCode,
            "Contained",
            ct
        );
        await EscapesHandler.ReportEscapeAsync(client, customerCode, errorCode, "Still open", ct);

        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        var resolvedAt = factory.Clock.GetUtcNow();
        await EscapesHandler.ResolveEscapeAsync(client, resolved, "Replacement shipped", ct);

        var open = await EscapesHandler.GetEscapesAsync(
            client,
            $"customerCode={customerCode}&isResolved=false",
            ct
        );
        var closed = await EscapesHandler.GetEscapesAsync(
            client,
            $"customerCode={customerCode}&isResolved=true",
            ct
        );

        Assert.Equal(["Still open"], [.. open.Items.Select(i => i.Description)]);
        Assert.Equal(["Contained"], [.. closed.Items.Select(i => i.Description)]);
        Assert.Equal(resolvedAt, closed.Items[0].ResolvedAt);
    }

    [Fact]
    public async Task Rejects_an_invalid_filter_with_a_problem_document()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateLineOperatorClient()
            .GetAsync($"{EscapesEndpoint}?severity=nope", ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.BadRequest, "Request.Malformed"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_an_anonymous_request_with_401()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory.CreateAnonymousClient().GetAsync(EscapesEndpoint, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
