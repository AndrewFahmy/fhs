using System.Net.Http.Json;
using FHS.Api.Enums;
using FHS.Api.Features.ErrorCodes;
using Fhs.IntegrationTests.Extensions;

namespace Fhs.IntegrationTests.Handlers;

internal static class ErrorCodesHandler
{
    public static async Task<Guid> CreateErrorCodeAsync(
        HttpClient client,
        string code,
        Severity severity,
        CancellationToken ct,
        string? description = null
    )
    {
        var response = await client.PostAsJsonAsync(
            "/error-codes",
            new
            {
                code,
                description = description ?? code,
                severity = severity.ToString()
            },
            ct
        );

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateErrorCodeResponse>(ct);

        return created!.ErrorCodeId;
    }

    public static async Task RetireErrorCodeAsync(HttpClient client, Guid id, CancellationToken ct)
    {
        var response = await client.PostAsync($"/error-codes/{id}/retire", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<IReadOnlyList<ErrorCodeListItem>> GetErrorCodesAsync(
        HttpClient client,
        bool includeRetired,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/error-codes?includeRetired={includeRetired}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<IReadOnlyList<ErrorCodeListItem>>(ct))!;
    }
}
