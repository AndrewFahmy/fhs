using System.Reflection;
using FHS.Api.Data;
using FHS.Api.Interfaces;
using FHS.Api.Links;
using FHS.Api.Primitives;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace FHS.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDbContexts(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString(AppConstants.Data.DatabaseConnectionName);
            var isDevelopment = environment.IsDevelopment();

            // since both command and query DbContexts point to the same database we decided to just use one
            // but when QueryDbContext point to a different database (e.g: a read replica), please add a separate check call then.
            services.AddHealthChecks().AddDbContextCheck<FhsCommandDbContext>();

            AddDbContextInternal<FhsCommandDbContext>(services, connectionString, isDevelopment);
            AddDbContextInternal<FhsQueryDbContext>(services, connectionString, isDevelopment);

            return services;
        }

        public IServiceCollection AddJwtAuthentication(IConfiguration config, IWebHostEnvironment env)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = config[AppConstants.Auth.AuthorityPropertyName];
                options.Audience = config[AppConstants.Auth.AudiencePropertyName];
                options.RequireHttpsMetadata = !env.IsDevelopment();
                options.TokenValidationParameters.RoleClaimType = "roles";
            });

            services.AddAuthorization(ConfigureAuthorization)
                .AddHttpContextAccessor()
                .AddScoped<ICurrentUser, CurrentUser>()
                .AddScoped<IActorDirectory, ActorDirectory>();

            return services;
        }

        public IServiceCollection AddValidationByAssembly(Assembly assembly)
        {
            services.AddValidatorsFromAssembly(assembly);
            services.AddScoped(typeof(ValidateRequestInput<>));

            return services;
        }

        public IServiceCollection AddOpenApiDocument()
        {
            services.AddOpenApi(options => 
                options.AddDocumentTransformer(
                    (document, context, ct) =>
                    {
                        document.Components ??= new OpenApiComponents();
                        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = 
                            new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.Http,
                                Scheme = "bearer",
                                BearerFormat = "JWT",
                                In = ParameterLocation.Header,
                                Description = "A Keycloak access token for the fhs realm."
                            };

                        document.Security =
                        [
                            new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                            }
                        ];

                        return Task.CompletedTask;
                    }
                )
            );

            return services;
        }

        public IServiceCollection AddSpaCors(IConfiguration config)
        {
            var allowedOrigins = config.GetValue<string[]?>(AppConstants.Cors.AllowedOriginsPropertyName) ?? [];
            
            services.AddCors(options => 
                options.AddPolicy(
                    AppConstants.Cors.SpaPolicy,
                    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()
                )
            );

            return services;
        }
    }

    private static void AddDbContextInternal<TDbContext>(
        IServiceCollection services,
        string? connectionString, 
        bool isDevelopment
    )  where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(connectionString, opts => opts.EnableRetryOnFailure());
            options.UseSnakeCaseNamingConvention();

            if (isDevelopment)
            {
                options.EnableDetailedErrors().EnableSensitiveDataLogging();
            }
        });
    }

    private static void ConfigureAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(
            AppConstants.Auth.AdminAccessPolicy,
            policy => policy.RequireRole(AppConstants.Auth.AdminRole)
        );
    }
}