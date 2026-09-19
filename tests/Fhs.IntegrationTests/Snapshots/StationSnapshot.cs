using FHS.Api.Data.Entities;
using FHS.IntegrationTests.Interfaces;

namespace FHS.IntegrationTests.Snapshots;

internal sealed record StationSnapshot(string Code, string Name, bool IsActive) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(Station);

    public static string[] ExcludedProperties { get; } = [nameof(Station.Id), nameof(Station.Version)];

    public static StationSnapshot From(Station station) => new(station.Code, station.Name, station.IsActive);
}
