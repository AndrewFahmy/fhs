namespace Fhs.IntegrationTests.Interfaces;

public interface ISnapshotModel
{
    static abstract Type RelatedType { get; }

    static abstract string[] ExcludedProperties { get; }

    public static Type GetRelatedType<TSnapshot>()
        where TSnapshot : ISnapshotModel => TSnapshot.RelatedType;

    public static string[] GetExcludedProperties<TSnapshot>()
        where TSnapshot : ISnapshotModel => TSnapshot.ExcludedProperties;
}
