using FHS.Api.Data.Entities;
using FHS.Api.Enums;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record ErrorCodeSnapshot(string Code, string Description, Severity Severity, bool IsActive)
    : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(ErrorCode);

    public static string[] ExcludedProperties { get; } = [nameof(ErrorCode.Id), nameof(ErrorCode.Version)];

    public static ErrorCodeSnapshot From(ErrorCode errorCode) =>
        new(errorCode.Code, errorCode.Description, errorCode.Severity, errorCode.IsActive);
}
