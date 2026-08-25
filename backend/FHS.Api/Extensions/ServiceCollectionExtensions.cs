using FHS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDbContexts(IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddDbContext<FhsCommandDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(AppConstants.Data.DatabaseConnectionName));
                options.UseSnakeCaseNamingConvention();

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors().EnableSensitiveDataLogging();
                }
            });

            services.AddDbContext<FhsQueryDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(AppConstants.Data.DatabaseConnectionName));
                options.UseSnakeCaseNamingConvention();


                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors().EnableSensitiveDataLogging();
                }
            });

            return services;
        }
    }
}