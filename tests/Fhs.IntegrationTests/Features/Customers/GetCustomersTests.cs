using FHS.Api.Features.Customers;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;

namespace Fhs.IntegrationTests.Features.Customers;

[Collection(nameof(FhsApiCollection))]
public sealed class GetCustomersTests(FhsApiFactory factory)
{
    [Fact]
    public async Task Lists_active_customers_in_code_order()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var prefix = adminClient.UniqueCode("CU");

        await CustomersHandler.CreateCustomerAsync(adminClient, $"{prefix}C", ct, "Third account");
        var a = await CustomersHandler.CreateCustomerAsync(adminClient, $"{prefix}A", ct, "First account");
        var b = await CustomersHandler.CreateCustomerAsync(adminClient, $"{prefix}B", ct, "Second account");

        var customers = await CustomersHandler.GetCustomersAsync(adminClient, includeInactive: false, ct);

        Assert.Equal(
            [$"{prefix}A", $"{prefix}B", $"{prefix}C"],
            [.. customers.Where(c => c.Code.StartsWith(prefix)).Select(c => c.Code)]
        );

        Assert.Contains(new CustomerListItem(a, $"{prefix}A", "First account", IsActive: true), customers);
        Assert.Contains(new CustomerListItem(b, $"{prefix}B", "Second account", IsActive: true), customers);
    }

    [Fact]
    public async Task Hides_deactivated_customers_unless_asked_for_them()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("CU");

        var customerId = await CustomersHandler.CreateCustomerAsync(adminClient, code, ct, "Closed account");
        await CustomersHandler.DeactivateCustomerAsync(adminClient, customerId, ct);

        var active = await CustomersHandler.GetCustomersAsync(adminClient, includeInactive: false, ct);
        var all = await CustomersHandler.GetCustomersAsync(adminClient, includeInactive: true, ct);

        Assert.DoesNotContain(active, c => c.CustomerId == customerId);
        Assert.Contains(new CustomerListItem(customerId, code, "Closed account", IsActive: false), all);
    }
}
