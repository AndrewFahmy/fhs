using System.Reflection;
using FHS.Api.Data;
using FHS.Api.Extensions;
using FHS.Chain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var assemblyReference = typeof(Program).Assembly;


{
    builder.AddServiceDefaults();

    builder.Services.AddSingleton(TimeProvider.System);

    builder
        .Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing.AddSource(ChainRunner.ActivitySourceName).AddNpgsql());

    builder.Services.AddProblemDetails();

    builder
        .Services.AddDbContexts(builder.Configuration, builder.Environment)
        .AddJwtAuthentication(builder.Configuration, builder.Environment)
        .AddValidationByAssembly(assemblyReference)
        .AddChain(assemblyReference);
}

var app = builder.Build();


{
    if (app.Environment.IsDevelopment())
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<FhsCommandDbContext>().Database.MigrateAsync();
    }

    app.UseExceptionHandler();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints();
    app.MapApiEndpoints();
}

app.Run();
