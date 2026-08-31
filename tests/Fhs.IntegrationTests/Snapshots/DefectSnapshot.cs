using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record DefectSnapshot(
    Guid StationId,
    Guid ErrorCodeId,
    string Description,
    Severity Severity,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    string? Resolution,
    Guid? ResolvedBy,
    DateTimeOffset? ResolvedAt
) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(Defect);

    public static string[] ExcludedProperties { get; } = [nameof(Defect.Id), nameof(Defect.Version)];

    public static DefectSnapshot From(Defect defect) =>
        new(
            defect.StationId,
            defect.ErrorCodeId,
            defect.Description,
            defect.Severity,
            defect.CreatedBy,
            defect.CreatedAt,
            defect.Resolution,
            defect.ResolvedBy,
            defect.ResolvedAt
        );
}
