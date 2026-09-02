using System.Net.Http.Json;
using Fhs.IntegrationTests.Extensions;
using FHS.Api.Features.Stations;

namespace Fhs.IntegrationTests.Handlers;

internal static class StationsHandler
{
    public static async Task<Guid> CreateStationAsync(
        HttpClient client,
        string code,
        CancellationToken ct,
        string? name = null
    )
    {
        var response = await client.PostAsJsonAsync("/stations", new { code, name = name ?? code }, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateStationResponse>(ct);

        return created!.StationId;
    }

    public static async Task DecommissionStationAsync(HttpClient client, Guid id, CancellationToken ct)
    {
        var response = await client.PostAsync($"/stations/{id}/decommission", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<IReadOnlyList<StationListItem>> GetStationAsync(
        HttpClient client,
        bool includeInactive,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/stations?includeInactive={includeInactive}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<IReadOnlyList<StationListItem>>(ct))!;
    }
}
