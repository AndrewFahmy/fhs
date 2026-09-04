using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record EscapeSnapshot(
    Guid CustomerId,
    Guid ErrorCodeId,
    string Description,
    Severity Severity,
    Guid ReportedBy,
    DateTimeOffset ReportedAt,
    string? Resolution,
    Guid? ResolvedBy,
    DateTimeOffset? ResolvedAt
) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(Escape);

    public static string[] ExcludedProperties { get; } =
        [
            nameof(Escape.Id),
            nameof(Escape.Version),
            nameof(Escape.Customer),
            nameof(Escape.ErrorCode),
            nameof(Escape.Reporter),
            nameof(Escape.Resolver)
        ];

    public static EscapeSnapshot From(Escape escape) =>
        new(
            escape.CustomerId,
            escape.ErrorCodeId,
            escape.Description,
            escape.Severity,
            escape.ReportedBy,
            escape.ReportedAt,
            escape.Resolution,
            escape.ResolvedBy,
            escape.ResolvedAt
        );
}
