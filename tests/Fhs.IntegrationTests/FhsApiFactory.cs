using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace Fhs.IntegrationTests;

public sealed class FhsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string OperatorSubjectId = AppConstants.Data.LineOperatorSubjectId;
    public static readonly Guid OperatorActorId = AppConstants.Data.LineOperatorActorId;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("fhs-db")
        .Build();

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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DbConnection"] = _postgres.GetConnectionString(),
                    ["Authentication:Authority"] = "https://keycloak.invalid/realms/fhs",
                    ["Authentication:Audience"] = "fhs-api"
                }
            )
        );

        builder.ConfigureTestServices(services =>
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { })
        );
    }

    public HttpClient CreateAnonymousClient() => CreateClient();

    public HttpClient CreateOperatorClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubjectHeader, OperatorSubjectId);

        return client;
    }

    public HttpClient CreateClientAs(string subjectId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubjectHeader, subjectId);

        return client;
    }

    /// <summary>The single database seam. Feature-specific helpers are extension methods over this.</summary>
    public async Task<T> ExecuteAsync<T>(
        Func<FhsCommandDbContext, CancellationToken, Task<T>> action,
        CancellationToken ct
    )
    {
        await using var scope = Services.CreateAsyncScope();

        return await action(scope.ServiceProvider.GetRequiredService<FhsCommandDbContext>(), ct);
    }

    public Task ExecuteAsync(
        Func<FhsCommandDbContext, CancellationToken, Task> action,
        CancellationToken ct
    ) =>
        ExecuteAsync<object?>(
            async (db, token) =>
            {
                await action(db, token);
                return null;
            },
            ct
        );
}
