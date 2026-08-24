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
            type.Name,
            Fields<RequiresAttribute>(type, a => a.Fields),
            Fields<ProducesAttribute>(type, a => a.Fields)
        );
    }

    private static string[] Fields<TAttribute>(Type type, Func<TAttribute, IReadOnlyList<string>> select)
        where TAttribute : Attribute =>
        [.. type.GetCustomAttributes<TAttribute>(inherit: false).SelectMany(select)];
}
