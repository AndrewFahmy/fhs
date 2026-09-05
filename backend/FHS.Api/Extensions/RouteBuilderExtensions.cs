using System.Reflection;
using FHS.Api.Interfaces;
using FHS.Api.Primitives;

namespace FHS.Api.Extensions;

public static class RouteBuilderExtensions
{

    private const string ProblemContentType = "application/problem+json";
    extension(IEndpointRouteBuilder app)
    {
        public void MapApiEndpoints()
        {
            var group = app.MapGroup("")
                .WithMetadata(
                    new ProducesResponseTypeMetadata(StatusCodes.Status401Unauthorized, typeof(void))
                )
                .WithMetadata(
                    new ProducesResponseTypeMetadata(
                        StatusCodes.Status500InternalServerError,
                        typeof(FhsProblemDetails),
                        [ProblemContentType]
                    )
                );

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

    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder ProducesProblems(params int[] statusCodes)
        {
            foreach (var statusCode in statusCodes)
            {
                builder.Produces<FhsProblemDetails>(statusCode, ProblemContentType);
            }

            return builder;
        }
    }

    // Private Methods
    private static void RegisterEndpoint<TEndpoint>(IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.MapEndpoint(app);
    }
}