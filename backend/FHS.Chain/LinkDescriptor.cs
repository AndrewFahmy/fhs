using System.Reflection;
using FHS.Chain.Attributes;

namespace FHS.Chain;

public sealed record LinkDescriptor(
    Type Type,
    string Name,
    IReadOnlyList<string> Requires,
    IReadOnlyList<string> Produces
)
{
    internal static LinkDescriptor For<TLink>()
    {
        var type = typeof(TLink);

        return new LinkDescriptor(
            type,
            NameOf(type),
            Fields<RequiresAttribute>(type, a => a.Fields),
            Fields<ProducesAttribute>(type, a => a.Fields)
        );
    }

    private static string[] Fields<TAttribute>(Type type, Func<TAttribute, IReadOnlyList<string>> select)
        where TAttribute : Attribute =>
        [.. type.GetCustomAttributes<TAttribute>(inherit: false).SelectMany(select)];

    private static string NameOf(Type type) =>
        type.IsGenericType
            ? $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(a => a.Name))}>"
            : type.Name;
}
