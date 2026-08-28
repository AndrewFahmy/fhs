using System.Reflection;
using FHS.Api.Interfaces;
using FHS.Chain;
using FHS.Chain.Contracts;

namespace Fhs.ArchitectureTests;

internal sealed record DeclaredChain(string Endpoint, string Name, IReadOnlyList<LinkDescriptor> Links);

internal static class ChainCatalog
{
    internal static readonly Assembly ApiAssembly = typeof(IEndpoint).Assembly;

    /// <summary>
    /// Every chain declared in the API, found by reading the static Chain fields on endpoint types.
    /// Reading the field forces the static initializer, which is what runs Build() — and therefore
    /// the wiring check — so simply calling this method verifies every chain's ordering.
    /// </summary>
    internal static IReadOnlyList<DeclaredChain> All() =>
        [
            .. ApiAssembly
                .GetTypes()
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t)
                )
                .SelectMany(endpoint =>
                    endpoint
                        .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(f => IsChain(f.FieldType))
                        .Select(f => Describe(endpoint.Name, f.GetValue(null)!))
                ),
        ];

    internal static IEnumerable<Type> LinkTypes() =>
        ApiAssembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && ImplementsLink(t));

    internal static bool ImplementsLink(Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ILink<>));

    internal static bool IsLinkContract(Type type) =>
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ILink<>)) || ImplementsLink(type);

    private static DeclaredChain Describe(string endpoint, object chain)
    {
        var type = chain.GetType();

        return new(
            endpoint,
            (string)type.GetProperty(nameof(Chain<>.Name))!.GetValue(chain)!,
            (IReadOnlyList<LinkDescriptor>)type.GetProperty(nameof(Chain<>.Links))!.GetValue(chain)!
        );
    }

    private static bool IsChain(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Chain<>))
            {
                return true;
            }
        }

        return false;
    }
}
