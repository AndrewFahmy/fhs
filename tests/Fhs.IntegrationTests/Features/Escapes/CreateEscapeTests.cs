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
public sealed class CreateEscapeTests(FhsApiFactory factory)
{
    private const string EscapesEndpoint = "/escapes";

    [Fact]
    public async Task Persists_the_escape_and_records_the_domain_event()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        var customerId = await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            errorCode,
            Severity.Critical,
            ct
        );
        var reportedAt = factory.Clock.GetUtcNow();

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                EscapesEndpoint,
                new
                {
                    customerCode,
                    errorCode,
                    description = "Paint run found at customer"
                },
                ct
            );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CreateEscapeResponse>(ct);
        Assert.NotNull(created);
        Assert.Equal($"/escapes/{created.EscapeId}", response.Headers.Location?.ToString());

        var escape = await factory.FindAsync<Escape>(created.EscapeId, ct);
        Assert.NotNull(escape);

        Assert.Equal(
            new EscapeSnapshot(
                customerId,
                errorCodeId,
                "Paint run found at customer",
                Severity.Critical,
                AppConstants.Data.LineOperatorActorId,
                ReportedAt: reportedAt,
                Resolution: null,
                ResolvedBy: null,
                ResolvedAt: null
            ),
            EscapeSnapshot.From(escape)
        );

        Assert.Equal(
            [
                OutboxMessageSnapshot.For(
                    new EscapeReported(
                        created.EscapeId,
                        customerId,
                        errorCodeId,
                        Severity.Critical,
                        reportedAt
                    )
                )
            ],
            [.. (await factory.OutboxForAsync(created.EscapeId, ct)).Select(OutboxMessageSnapshot.From)]
        );
    }

    [Fact]
    public async Task Reports_every_shape_failure_at_once()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                EscapesEndpoint,
                new
                {
                    customerCode = "",
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
    public async Task Returns_404_when_the_customer_does_not_exist()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var errorCode = adminClient.UniqueCode("EC");

        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                EscapesEndpoint,
                new
                {
                    customerCode = "NO-SUCH-CUSTOMER",
                    errorCode,
                    description = "x"
                },
                ct
            );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Escapes.CustomerNotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Returns_409_when_the_customer_is_deactivated()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        var customerId = await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        await ErrorCodesHandler.CreateErrorCodeAsync(adminClient, errorCode, Severity.Major, ct);
        await CustomersHandler.DeactivateCustomerAsync(adminClient, customerId, ct);

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsJsonAsync(
                EscapesEndpoint,
                new
                {
                    customerCode,
                    errorCode,
                    description = "x"
                },
                ct
            );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Escapes.CustomerInactive"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Returns_409_when_the_error_code_is_retired()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();

        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
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
                EscapesEndpoint,
                new
                {
                    customerCode,
                    errorCode,
                    description = "x"
                },
                ct
            );

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Escapes.ErrorCodeInactive"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }
}
