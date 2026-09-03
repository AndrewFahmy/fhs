using FHS.Api.Enums;
using FHS.Api.Features.ErrorCodes;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Handlers;

namespace Fhs.IntegrationTests.Features.ErrorCodes;

[Collection(nameof(FhsApiCollection))]
public sealed class GetErrorCodesTests(FhsApiFactory factory)
{
    [Fact]
    public async Task Lists_active_error_codes_in_code_order()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var prefix = adminClient.UniqueCode("EC");

        await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            $"{prefix}C",
            Severity.Minor,
            ct,
            "Third fault"
        );
        var a = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            $"{prefix}A",
            Severity.Critical,
            ct,
            "First fault"
        );
        await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            $"{prefix}B",
            Severity.Major,
            ct,
            "Second fault"
        );

        var errorCodes = await ErrorCodesHandler.GetErrorCodesAsync(adminClient, includeInactive: false, ct);

        Assert.Equal(
            [$"{prefix}A", $"{prefix}B", $"{prefix}C"],
            [.. errorCodes.Where(e => e.Code.StartsWith(prefix)).Select(e => e.Code)]
        );

        Assert.Contains(
            new ErrorCodeListItem(a, $"{prefix}A", "First fault", Severity.Critical, IsActive: true),
            errorCodes
        );
    }

    [Fact]
    public async Task Hides_retired_error_codes_unless_asked_for_them()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var code = adminClient.UniqueCode("EC");

        var errorCodeId = await ErrorCodesHandler.CreateErrorCodeAsync(
            adminClient,
            code,
            Severity.Major,
            ct,
            "Retired fault"
        );
        await ErrorCodesHandler.RetireErrorCodeAsync(adminClient, errorCodeId, ct);

        var active = await ErrorCodesHandler.GetErrorCodesAsync(adminClient, includeInactive: false, ct);
        var all = await ErrorCodesHandler.GetErrorCodesAsync(adminClient, includeInactive: true, ct);

        Assert.DoesNotContain(active, e => e.ErrorCodeId == errorCodeId);
        Assert.Contains(
            new ErrorCodeListItem(errorCodeId, code, "Retired fault", Severity.Major, IsActive: false),
            all
        );
    }
}
