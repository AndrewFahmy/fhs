using FHS.Api.Features.Stations;
using FHS.IntegrationTests.Extensions;
using FHS.IntegrationTests.Handlers;

namespace FHS.IntegrationTests.Features.Stations;

[Collection(nameof(FhsApiCollection))]
public sealed class GetStationsTests(FhsApiFactory factory)
{
    [Fact]
    public async Task Lists_active_stations_in_code_order()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var prefix = adminClient.UniqueCode("ST");
        var facilityCode = AppConstants.Data.DefaultFacilityCode;

        // Created out of order so the ordering assertion means something, and suffixed with letters
        // rather than punctuation so no database collation rule can reorder them.
        await StationsHandler.CreateStationAsync(adminClient, $"{prefix}C", facilityCode, ct, "Third bay");
        var a = await StationsHandler.CreateStationAsync(
            adminClient,
            $"{prefix}A",
            facilityCode,
            ct,
            "First bay"
        );
        var b = await StationsHandler.CreateStationAsync(
            adminClient,
            $"{prefix}B",
            facilityCode,
            ct,
            "Second bay"
        );

        var stations = await StationsHandler.GetStationAsync(adminClient, includeInactive: false, ct);

        Assert.Equal(
            [$"{prefix}A", $"{prefix}B", $"{prefix}C"],
            [.. stations.Where(s => s.Code.StartsWith(prefix)).Select(s => s.Code)]
        );

        Assert.Contains(new StationListItem(a, $"{prefix}A", "First bay", IsActive: true), stations);
        Assert.Contains(new StationListItem(b, $"{prefix}B", "Second bay", IsActive: true), stations);
    }

    [Fact]
    public async Task Hides_decommissioned_stations_unless_asked_for_them()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.CreateAdminClient();
        var code = adminClient.UniqueCode("ST");
        var facilityCode = AppConstants.Data.DefaultFacilityCode;

        var stationId = await StationsHandler.CreateStationAsync(
            adminClient,
            code,
            facilityCode,
            ct,
            "Retired bay"
        );
        await StationsHandler.DecommissionStationAsync(adminClient, stationId, ct);

        var active = await StationsHandler.GetStationAsync(adminClient, includeInactive: false, ct);
        var all = await StationsHandler.GetStationAsync(adminClient, includeInactive: true, ct);

        Assert.DoesNotContain(active, s => s.StationId == stationId);
        Assert.Contains(new StationListItem(stationId, code, "Retired bay", IsActive: false), all);
    }
}
