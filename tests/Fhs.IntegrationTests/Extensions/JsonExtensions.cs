using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fhs.IntegrationTests.Extensions;

internal static class JsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    extension(HttpResponseMessage response)
    {
        public Task<T?> ReadAsync<T>(CancellationToken ct) =>
            response.Content.ReadFromJsonAsync<T>(Options, ct);
    }
}