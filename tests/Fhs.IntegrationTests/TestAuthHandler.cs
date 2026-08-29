using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhs.IntegrationTests;

/// <summary>
/// Replaces JwtBearer as the default scheme. Everything downstream is untouched — RequireAuthorization
/// still runs, CurrentUser still reads ClaimTypes.NameIdentifier, and ResolveActor still has to find a
/// matching Actor row. Only token validation itself is skipped.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string SubjectHeader = "X-Test-Subject";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (
            !Request.Headers.TryGetValue(SubjectHeader, out var subject) || string.IsNullOrWhiteSpace(subject)
        )
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity([new(ClaimTypes.NameIdentifier, subject!)], SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName))
        );
    }
}
