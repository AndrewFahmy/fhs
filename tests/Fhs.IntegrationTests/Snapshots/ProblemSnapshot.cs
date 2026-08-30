using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record ProblemSnapshot(HttpStatusCode StatusCode, string Code)
{
    public static async Task<ProblemSnapshot> FromAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return new(response.StatusCode, body.GetProperty("code").GetString()!);
    }
}

internal sealed record ValidationProblemSnapshot(HttpStatusCode StatusCode, string Code, int ErrorCount)
{
    public static async Task<ValidationProblemSnapshot> FromAsync(
        HttpResponseMessage response,
        CancellationToken ct
    )
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return new(
            response.StatusCode,
            body.GetProperty("code").GetString()!,
            body.GetProperty("errors").GetArrayLength()
        );
    }
}
