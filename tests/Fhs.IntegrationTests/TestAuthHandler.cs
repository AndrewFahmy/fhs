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
    public const string RolesHeader = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (
            !Request.Headers.TryGetValue(SubjectHeader, out var subject) || string.IsNullOrWhiteSpace(subject)
        )
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        List<Claim> claims = [new(AppConstants.Auth.SubjectClaimType, subject!)];

        if (Request.Headers.TryGetValue(RolesHeader, out var roles))
        {
            claims.AddRange(
                roles
                    .ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(role => new Claim(AppConstants.Auth.RoleClaimType, role))
            );
        }

        // Same role claim type the JwtBearer handler is configured with, so RequireRole behaves identically.
        var identity = new ClaimsIdentity(
            claims,
            SchemeName,
            ClaimTypes.Name,
            AppConstants.Auth.RoleClaimType
        );

        return Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName))
        );
    }
}
