using System.Net;
using System.Net.Http.Json;
using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Snapshots;

namespace Fhs.IntegrationTests.Features.ErrorCodes;

[Collection(nameof(FhsApiCollection))]
public sealed class CreateErrorCodeTests(FhsApiFactory factory)
{
    private const string ErrorCodesEndpoint = $"/error-codes";

    [Fact]
    public async Task Persists_the_error_code_and_rejects_a_duplicate_code()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();
        var code = adminClient.UniqueCode("EC");
        var description = "Paint runs on the outer skin";

        // Sends "Critical", not an integer - proves that JsonStringEnumConverter is wired up.
        var errorCodeId = await adminClient.CreateErrorCodeAsync(code, Severity.Critical, ct, description);

        var duplicate = await adminClient.PostAsJsonAsync(
            ErrorCodesEndpoint,
            new
            {
                code,
                description,
                Severity = Severity.Critical,
            },
            ct
        );

        var errorCode = await factory.FindAsync<ErrorCode>(errorCodeId, ct);
        Assert.NotNull(errorCode);

        Assert.Equal(
            new ErrorCodeSnapshot(code, description, Severity.Critical, IsActive: true),
            ErrorCodeSnapshot.From(errorCode)
        );
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.Conflict, "ErrorCodes.CodeAlreadyExists"),
            await ProblemSnapshot.FromAsync(duplicate, ct)
        );
    }

    [Fact]
    public async Task Reports_every_shape_failure_at_once()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await factory
            .AdminClient()
            .PostAsJsonAsync(
                ErrorCodesEndpoint,
                new
                {
                    Code = "",
                    Severity = nameof(Severity.Major),
                    description = ""
                },
                ct
            );

        Assert.Equal(
            new ValidationProblemSnapshot(HttpStatusCode.BadRequest, "Validation.Failed", ErrorCount: 2),
            await ValidationProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Rejects_a_severity_name_the_enum_does_not_define()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();

        var response = await adminClient.PostAsJsonAsync(
            ErrorCodesEndpoint,
            new
            {
                code = adminClient.UniqueCode("EC"),
                description = "x",
                severity = "Catastrophic"
            },
            ct
        );

        // Rejected by the JSON reader before the chain runs, so the code is Request.Malformed
        // rather than Validation.Failed — but the body shape is the same.
        Assert.Equal(
            new ProblemSnapshot(HttpStatusCode.BadRequest, "Request.Malformed"),
            await ProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Rejects_a_severity_number_outside_the_enum()
    {
        var ct = TestContext.Current.CancellationToken;
        var adminClient = factory.AdminClient();

        var response = await adminClient.PostAsJsonAsync(
            ErrorCodesEndpoint,
            new
            {
                code = adminClient.UniqueCode("EC"),
                description = "x",
                severity = 99
            },
            ct
        );

        // The numeric path deserializes cleanly, so this is the only case IsInEnum() actually sees.
        Assert.Equal(
            new ValidationProblemSnapshot(HttpStatusCode.BadRequest, "Validation.Failed", ErrorCount: 1),
            await ValidationProblemSnapshot.FromAsync(response, ct)
        );
    }

    [Fact]
    public async Task Refuses_a_line_operator_with_403_rather_than_401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.LineOperatorClient();

        var response = await client.PostAsJsonAsync(
            ErrorCodesEndpoint,
            new
            {
                code = client.UniqueCode("EC"),
                description = "x",
                severity = nameof(Severity.Major)
            },
            ct
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
