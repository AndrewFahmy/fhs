using System.Net.Http.Json;
using FHS.Api.Features.Customers;
using Fhs.IntegrationTests.Extensions;

namespace Fhs.IntegrationTests.Handlers;

internal static class CustomersHandler
{
    public static async Task<Guid> CreateCustomerAsync(
        HttpClient client,
        string code,
        CancellationToken ct,
        string? name = null
    )
    {
        var response = await client.PostAsJsonAsync("/customers", new { code, name = name ?? code }, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateCustomerResponse>(ct);

        return created!.CustomerId;
    }

    public static async Task DeactivateCustomerAsync(HttpClient client, Guid id, CancellationToken ct)
    {
        var response = await client.PostAsync($"/customers/{id}/deactivate", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<IReadOnlyList<CustomerListItem>> GetCustomersAsync(
        HttpClient client,
        bool includeInactive,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/customers?includeInactive={includeInactive}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<IReadOnlyList<CustomerListItem>>(ct))!;
    }
}
