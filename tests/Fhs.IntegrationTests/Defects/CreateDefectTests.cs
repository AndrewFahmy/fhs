using System.Net;
using System.Net.Http.Json;
using FHS.Api.Enums;
using FHS.Api.Features.Defects;

namespace Fhs.IntegrationTests.Defects;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateDefectTests(FhsApiFactory factory)
{
    // [Fact]
    // public async Task Persists_the_defect_and_records_the_domain_event()
    // {
    //     var ct = TestContext.Current.CancellationToken;
    //     var (stationCode, errorCode) = await SeedAsync(Severity.Critical);

    //     var response = await factory
    //         .CreateOperatorClient()
    //         .PostAsJsonAsync(
    //             "/defects",
    //             new
    //             {
    //                 stationCode,
    //                 errorCode,
    //                 description = "Scratch on door panel"
    //             },
    //             ct
    //         );

    //     Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    //     var created = await response.Content.ReadFromJsonAsync<CreateDefectResponse>(ct);
    //     Assert.NotNull(created);
    //     Assert.Equal($"/defects/{created.DefectId}", response.Headers.Location?.ToString());
    // }
}
