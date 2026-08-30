using FHS.Api.Data.Entities;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record OutboxMessageSnapshot(string Type, bool IsProcessed) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(OutboxMessage);

    public static string[] ExcludedProperties { get; } =
        [
            nameof(OutboxMessage.Id),
            nameof(OutboxMessage.OccurredAt),
            nameof(OutboxMessage.Payload),
            nameof(OutboxMessage.ProcessedAt)
        ];

    public static OutboxMessageSnapshot From(OutboxMessage outboxMessage) =>
        new(outboxMessage.Type, outboxMessage.ProcessedAt is not null);

    public static OutboxMessageSnapshot For<TEvent>(bool isProcessed = false) =>
        new(typeof(TEvent).FullName!, isProcessed);
}
