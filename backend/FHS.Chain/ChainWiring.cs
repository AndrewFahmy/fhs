using FHS.Chain.Exceptions;

namespace FHS.Chain;

internal static class ChainWiring
{
    internal static string NameOf(Type stateType) => stateType.DeclaringType?.Name ?? stateType.Name;

    internal static void Verify(string name, IReadOnlyList<LinkDescriptor> links)
    {
        var produced = new HashSet<string>(StringComparer.Ordinal);

        foreach (var link in links)
        {
            foreach (var required in link.Requires)
            {
                if (!produced.Contains(required))
                    throw new ChainWiringException(
                        $"{name}: '{link.Name}' requires '{required}', which no earlier link produces."
                    );
            }

            foreach (var field in link.Produces)
            {
                if (!produced.Add(field))
                    throw new ChainWiringException(
                        $"{name}: '{link.Name}' produces '{field}', which an earlier link already produces."
                    );
            }
        }
    }
}
