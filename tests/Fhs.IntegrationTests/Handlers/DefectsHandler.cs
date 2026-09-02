using System.Net.Http.Json;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;
using FHS.Api.Primitives;
using Fhs.IntegrationTests.Extensions;

namespace Fhs.IntegrationTests.Handlers;

internal static class DefectsHandler
{
    public static async Task<(Guid DefectId, Guid StationId, Guid ErrorCodeId)> RaiseDefectAsync(
        HttpClient client,
        FhsApiFactory factory,
        CancellationToken ct
    )
    {
        var adminClient = factory.AdminClient();
        var stationCode = adminClient.UniqueCode("ST");
        var errorCode = adminClient.UniqueCode("EC");

        var stationId = await adminClient.CreateStationAsync(stationCode, ct);
        var errorCodeId = await adminClient.CreateErrorCodeAsync(errorCode, Severity.Major, ct);

        var response = await client.PostAsJsonAsync(
            "/defects",
            new
            {
                stationCode,
                errorCode,
                description = "Scratch on door panel"
            },
            ct
        );

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateDefectResponse>(ct);

        return (created!.DefectId, stationId, errorCodeId);
    }

    public static async Task<Guid> RaiseDefectAsync(
        HttpClient client,
        string stationCode,
        string errorCode,
        string description,
        CancellationToken ct
    )
    {
        var response = await client.PostAsJsonAsync(
            "/defects",
            new
            {
                stationCode,
                errorCode,
                description
            },
            ct
        );

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateDefectResponse>(ct);

        return created!.DefectId;
    }

    public static async Task ResolveDefectAsync(
        HttpClient client,
        Guid defectId,
        string resolution,
        CancellationToken ct
    )
    {
        var response = await client.PostAsJsonAsync($"/defects/{defectId}/resolve", new { resolution }, ct);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<PagedResponse<DefectListItem>> GetDefectsAsync(
        HttpClient client,
        string query,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/defects?{query}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<PagedResponse<DefectListItem>>(ct))!;
    }

    public static async Task<DefectDetailResponse> GetDefectAsync(
        HttpClient client,
        Guid defectId,
        CancellationToken ct
    )
    {
        var response = await client.GetAsync($"/defects/{defectId}", ct);
        response.EnsureSuccessStatusCode();

        return (await response.ReadAsync<DefectDetailResponse>(ct))!;
    }
}
