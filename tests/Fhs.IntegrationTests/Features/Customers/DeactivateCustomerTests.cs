using System.Net;
using FHS.Api.Data.Entities;
using FHS.Api.Features.Customers;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Customers;

[Collection(nameof(FhsApiCollection))]
public sealed class DeactivateCustomerTests(FhsApiFactory factory)
{
    private static string EndpointRoute(Guid customerId) => $"/customers/{customerId}/deactivate";

    [Fact]
    public async Task Deactivates_once_then_conflicts()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("CU");
        var name = $"{code} motors";
        var deactivatedAt = factory.Clock.GetUtcNow();

        var customerId = await CustomersHandler.CreateCustomerAsync(adminClient, code, ct, name);

        var first = await adminClient.PostAsync(EndpointRoute(customerId), null, ct);
        var second = await adminClient.PostAsync(EndpointRoute(customerId), null, ct);

        var customer = await factory.FindAsync<Customer>(customerId, ct);
        Assert.NotNull(customer);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(new CustomerSnapshot(code, name, IsActive: false), CustomerSnapshot.From(customer));
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "Customers.AlreadyDeactivated"),
            await ProblemSnapshot.FromAsync(second, ct)
        );
        Assert.Equal(
            [
                OutboxMessageSnapshot.For(
                    new CustomerDeactivated(customerId, AppConstants.Data.AdminActorId, deactivatedAt)
                )
            ],
            [.. (await factory.OutboxForAsync(customerId, ct)).Select(OutboxMessageSnapshot.From)]
        );
    }

    [Fact]
    public async Task Returns_404_for_an_unknown_customer()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateAdminClient()
            .PostAsync(EndpointRoute(Guid.CreateVersion7()), null, ct);

        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.NotFound, "Customers.NotFound"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_a_line_operator_with_403_rather_than_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var customerId = await CustomersHandler.CreateCustomerAsync(
            adminClient,
            adminClient.UniqueCode("CU"),
            ct
        );

        var response = await factory
            .CreateLineOperatorClient()
            .PostAsync(EndpointRoute(customerId), null, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
