using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.Customers;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateCustomerTests(FhsApiFactory factory)
{
    private const string CreateCustomerEndpoint = "/customers";

    [Fact]
    public async Task Persists_the_customer_and_rejects_a_duplicate_code()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("CU");
        var name = $"{code} motors";

        var customerId = await CustomersHandler.CreateCustomerAsync(adminClient, code, ct, name);

        var duplicate = await adminClient.PostAsJsonAsync(CreateCustomerEndpoint, new { code, name }, ct);

        var customer = await factory.FindAsync<Customer>(customerId, ct);
        Assert.NotNull(customer);

        Assert.Equal(new CustomerSnapshot(code, name, IsActive: true), CustomerSnapshot.From(customer));
        Assert.Equal(
            new ProblemSnapshot(System.Net.HttpStatusCode.Conflict, "Customers.CodeAlreadyExists"),
            await ProblemSnapshot.FromAsync(duplicate, ct)
        );
    }

    [Fact]
    public async Task Reports_every_shape_failure_at_once()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .CreateAdminClient()
            .PostAsJsonAsync(CreateCustomerEndpoint, new { code = "", name = "" }, ct);

        Assert.Equal(
            new ValidationProblemSnapshot(HttpStatusCode.BadRequest, "Validation.Failed", ErrorCount: 2),
            await ValidationProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_a_line_operator_with_403_rather_than_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateLineOperatorClient();
        var code = client.UniqueCode("CU");

        var response = await client.PostAsJsonAsync(CreateCustomerEndpoint, new { code, name = code }, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refuses_an_anonymous_request_with_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateAnonymousClient();
        var code = client.UniqueCode("CU");

        var response = await client.PostAsJsonAsync(CreateCustomerEndpoint, new { code, name = code }, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
