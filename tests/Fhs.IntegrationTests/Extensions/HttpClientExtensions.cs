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

        public async Task<Guid> CreateStationAsync(string code, CancellationToken ct)
        {
            var response = await client.PostAsJsonAsync("/stations", new{ code, name = code }, ct);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<CreateStationResponse>(ct);

            return created!.StationId;
        }

        public async Task DecommissionStationAsync(Guid id, CancellationToken ct)
        {
            var response = await client.PostAsync($"/stations/{id}/decommission", null, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Guid> CreateErrorCodeAsync(string code, Severity severity, CancellationToken ct)
        {
            var response = await client.PostAsJsonAsync("/error-codes", new { code, description = code, severity = severity.ToString() }, ct);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<CreateErrorCodeResponse>(ct);
            
            return created!.ErrorCodeId;
        }

        public async Task RetireErrorCodeAsync(Guid id, CancellationToken ct)
        {
            var response = await client.PostAsync($"/error-codes/{id}/retire", null, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task<(Guid DefectId, Guid StationId, Guid ErrorCodeId)> RaiseDefectAsync(FhsApiFactory factory, CancellationToken ct)
        {
            var adminClient = factory.AdminClient();
            var stationCode = adminClient.UniqueCode("ST");
            var errorCode = adminClient.UniqueCode("EC");

            var stationId = await adminClient.CreateStationAsync(stationCode, ct);
            var errorCodeId = await adminClient.CreateErrorCodeAsync(errorCode, Severity.Major, ct);

            var response = await client.PostAsJsonAsync("/defects",
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
    }
}
