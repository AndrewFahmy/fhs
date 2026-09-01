using FHS.Api.Interfaces;

namespace FHS.Api.Data.Entities;

public sealed class OutboxMessage : IDbEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Type { get; init; }

    public required string Payload { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public DateTimeOffset? ProcessedAt { get; set; }
}
