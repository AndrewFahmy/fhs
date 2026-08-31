using FHS.Api.Data.Entities;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record OutboxMessageSnapshot(string Type, DateTimeOffset OccurredAt, bool IsProcessed)
    : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(OutboxMessage);

    public static string[] ExcludedProperties { get; } =
        [nameof(OutboxMessage.Id), nameof(OutboxMessage.Payload), nameof(OutboxMessage.ProcessedAt)];

    public static OutboxMessageSnapshot From(OutboxMessage outboxMessage) =>
        new(outboxMessage.Type, outboxMessage.OccurredAt, outboxMessage.ProcessedAt is not null);

    public static OutboxMessageSnapshot For<TEvent>(DateTimeOffset occurredAt, bool isProcessed = false) =>
        new(typeof(TEvent).FullName!, occurredAt, isProcessed);
}
