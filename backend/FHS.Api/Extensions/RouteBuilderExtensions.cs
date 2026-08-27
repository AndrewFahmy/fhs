using System.Reflection;
using FHS.Api.Interfaces;

namespace FHS.Api.Extensions;

public static class RouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapApiEndpoints()
        {
            var endpoints = typeof(Program)
                .Assembly.GetTypes()
                .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            var mappingMethodInfo = typeof(RouteBuilderExtensions).GetMethod(
                nameof(RegisterEndpoint),
                BindingFlags.NonPublic | BindingFlags.Static
            )!;

            foreach (var endpoint in endpoints)
            {
                mappingMethodInfo.MakeGenericMethod(endpoint).Invoke(null, [app]);
            }
        }
    }

    // Private Methods
    private static void RegisterEndpoint<TEndpoint>(IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.MapEndpoint(app);
    }
}