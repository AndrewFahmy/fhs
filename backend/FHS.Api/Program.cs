using System.Text.Json.Serialization;
using FHS.Api.Data;
using FHS.Api.Extensions;
using FHS.Api.Primitives;
using FHS.Chain;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var assemblyReference = typeof(Program).Assembly;


{
    builder.AddServiceDefaults();

    builder.Services.AddSingleton(TimeProvider.System);

    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

    builder
        .Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing.AddSource(ChainRunner.ActivitySourceName).AddNpgsql());

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();

    // Pinned rather than left to default: otherwise binding failures are a bodiless 400 in
    // production and a 500 in development, and the tests only ever see the development answer.
    builder.Services.PostConfigure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

    builder
        .Services.AddDbContexts(builder.Configuration, builder.Environment)
        .AddJwtAuthentication(builder.Configuration, builder.Environment)
        .AddSpaCors(builder.Configuration)
        .AddValidationByAssembly(assemblyReference)
        .AddOpenApiDocument()
        .AddChain(assemblyReference);

    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration)
    );
}

var app = builder.Build();


{
    if (app.Environment.IsDevelopment())
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<FhsCommandDbContext>().Database.MigrateAsync();

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.HideModels();
            options
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
        });
    }

    app.UseExceptionHandler();

    app.UseCors(AppConstants.Cors.SpaPolicy);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints();
    app.MapApiEndpoints();
}

app.Run();
