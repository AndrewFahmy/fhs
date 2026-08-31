using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace Fhs.IntegrationTests;

public sealed class FhsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithName("fhs-integration-tests-postgres")
        .WithDatabase("fhs-db")
        .Build();

    /// <summary>
    /// Frozen unless a test advances it, so every timestamp the API writes is exactly predictable.
    /// Whole seconds in UTC: Postgres' <c>timestamptz</c> keeps microseconds, so it round-trips intact.
    /// </summary>
    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Building the host runs Program.cs, including MigrateAsync.
        using var _ = CreateClient();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureHostConfiguration(config =>
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DbConnection"] = _postgres.GetConnectionString(),
                    ["Authentication:Authority"] = "https://keycloak.invalid/realms/fhs",
                    ["Authentication:Audience"] = "fhs-api"
                }
            )
        );

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { }
                );

            services.RemoveAll<TimeProvider>().AddSingleton<TimeProvider>(Clock);
        });
    }

    public HttpClient CreateAnonymousClient() => CreateClient();

    public HttpClient CreateClientAs(string subjectId, params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubjectHeader, subjectId);

        if (roles.Length > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(',', roles));
        }

        return client;
    }
}
