using System.Reflection;
using System.Text;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Features;

public sealed class SnapshotCoverageTests
{
    private static readonly MethodInfo RelatedTypeMappingMethod = typeof(ISnapshotModel).GetMethod(
        nameof(ISnapshotModel.GetRelatedType),
        BindingFlags.Public | BindingFlags.Static
    )!;

    private static readonly MethodInfo ExcludedPropertiesMappingMethod = typeof(ISnapshotModel).GetMethod(
        nameof(ISnapshotModel.GetExcludedProperties),
        BindingFlags.Public | BindingFlags.Static
    )!;

    [Fact]
    public void Snapshot_models_accounts_for_every_entity_property()
    {
        var types = typeof(ISnapshotModel)
            .Assembly.GetTypes()
            .Where(t => typeof(ISnapshotModel).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToArray();

        StringBuilder sb = new();
        int errorCount = 0;

        foreach (var type in types)
        {
            var accountedFor = type.GetProperties()
                .Select(p => p.Name)
                .Concat((string[])ExcludedPropertiesMappingMethod.MakeGenericMethod(type).Invoke(null, [])!)
                .ToHashSet();

            var missing = ((Type)RelatedTypeMappingMethod.MakeGenericMethod(type).Invoke(null, [])!)
                .GetProperties()
                .Select(p => p.Name)
                .Where(name => !accountedFor.Contains(name))
                .Where(name =>
                    !new[]
                    {
                        nameof(ISnapshotModel.RelatedType),
                        nameof(ISnapshotModel.ExcludedProperties)
                    }.Contains(name)
                )
                .ToArray();

            if (missing.Length > 0)
            {
                errorCount++;
                sb.AppendLine($"Type {type.Name} is missing properties: {string.Join(", ", missing)}");
                sb.AppendLine();
            }
        }

        Assert.True(errorCount == 0, $"Some snapshot models are missing properties:\n{sb}");
    }
}
