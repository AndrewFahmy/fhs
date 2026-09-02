using System.Net.Http.Json;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;
using FHS.Api.Features.ErrorCodes;
using FHS.Api.Features.Stations;

namespace Fhs.IntegrationTests.Extensions;

internal static class HttpClientExtensions
{
    extension (HttpClient client)
    {
        public string UniqueCode(string prefix) => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";
    }
}
