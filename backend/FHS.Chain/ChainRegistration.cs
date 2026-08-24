using System.Reflection;
using FHS.Chain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FHS.Chain;

public static class ChainRegistration
{
    public static IServiceCollection AddChain(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<ChainRunner>();

        var linkTypes = assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false })
            .Where(t =>
                t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ILink<>))
            );

        foreach (var link in linkTypes)
        {
            services.TryAddScoped(link);
        }

        return services;
    }
}
