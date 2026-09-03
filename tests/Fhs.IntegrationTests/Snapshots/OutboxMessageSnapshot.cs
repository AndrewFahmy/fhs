using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using Fhs.IntegrationTests.Extensions;
using Fhs.IntegrationTests.Interfaces;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record OutboxMessageSnapshot(
    string Type,
    DateTimeOffset OccurredAt,
    bool IsProcessed,
    IDomainEvent Payload
) : ISnapshotModel
{
    public static Type RelatedType { get; } = typeof(OutboxMessage);

    public static string[] ExcludedProperties { get; } =
        [nameof(OutboxMessage.Id), nameof(OutboxMessage.ProcessedAt)];

    public static OutboxMessageSnapshot From(OutboxMessage outboxMessage) =>
        new(
            outboxMessage.Type,
            outboxMessage.OccurredAt,
            outboxMessage.ProcessedAt is not null,
            outboxMessage.DeserializePayload()
        );

    public static OutboxMessageSnapshot For<TEvent>(TEvent @event, bool isProcessed = false)
        where TEvent : IDomainEvent
    {
        return new(typeof(TEvent).FullName!, @event.OccurredAt, isProcessed, @event);
    }
}
