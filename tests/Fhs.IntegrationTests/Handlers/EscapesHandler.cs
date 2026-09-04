using System.Net.Http.Json;
using FHS.Api.Enums;
using FHS.Api.Features.Escapes;
using FHS.Api.Primitives;
using Fhs.IntegrationTests.Extensions;

namespace Fhs.IntegrationTests.Handlers;

internal static class EscapesHandler
{
    public static async Task<(Guid EscapeId, Guid CustomerId, Guid ErrorCodeId)> ReportEscapeAsync(
        HttpClient client,
        FhsApiFactory factory,
        CancellationToken ct
    )
    {
        var adminClient = factory.CreateAdminClient();
        var customerCode = adminClient.UniqueCode("CU");
        var errorCode = adminClient.UniqueCode("EC");

        var customerId = await CustomersHandler.CreateCustomerAsync(adminClient, customerCode, ct);
        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            errorCode,
            Severity.Major,
            ct
        );

        var escapeId = await ReportEscapeAsync(
            client,
            customerCode,
            errorCode,
            "Paint run found at customer",
            ct
        );

        return (escapeId, customerId, errorCodeId);
    }

    public static async Task<Guid> ReportEscapeAsync(
        HttpClient client,
        string customerCode,
        string errorCode,
        string description,
        CancellationToken ct
    )
    {
        var response = await client.PostAsJsonAsync(
            "/escapes",
            new
            {
                customerCode,
                errorCode,
                description
            },
            ct
        );

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateEscapeResponse>(ct);

        return created!.EscapeId;
    }

    public static async Task ResolveEscapeAsync(
        HttpClient client,
        Guid escapeId,
        string resolution,
        CancellationToken ct
    )
    {
        var response = await client.PostAsJsonAsync($"/escapes/{escapeId}/resolve", new { resolution }, ct);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<PagedResponse<EscapeListItem>> GetEscapesAsync(
        HttpClient client,
        string query,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/escapes?{query}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<PagedResponse<EscapeListItem>>(ct))!;
    }

    public static async Task<EscapeDetailResponse> GetEscapeAsync(
        HttpClient client,
        Guid escapeId,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/escapes/{escapeId}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<EscapeDetailResponse>(ct))!;
    }
}
