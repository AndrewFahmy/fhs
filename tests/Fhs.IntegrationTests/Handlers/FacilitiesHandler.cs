using System.Net.Http.Json;
using FHS.Api.Features.Facilities;

namespace FHS.IntegrationTests.Handlers;

internal static class FacilitiesHandler
{
    public static async Task<Guid> CreateFacilityAsync(
        HttpClient client,
        string code,
        CancellationToken ct,
        string? name = null
    )
    {
        var response = await client.PostAsJsonAsync("/facilities", new { code, name = name ?? code }, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateFacilityResponse>(ct);

        return created!.FacilityId;
    }
}
