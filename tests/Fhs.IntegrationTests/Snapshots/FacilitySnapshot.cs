using FHS.Api.Data.Entities;
using FHS.IntegrationTests.Interfaces;

namespace FHS.IntegrationTests.Snapshots;

internal sealed record FacilitySnapshot(string Code, string Name, bool IsActive) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(Facility);

    public static string[] ExcludedProperties { get; } = [nameof(Facility.Id), nameof(Facility.Version)];

    public static FacilitySnapshot From(Facility facility) =>
        new(facility.Code, facility.Name, facility.IsActive);
}
