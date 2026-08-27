using System.Text.Json;
using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Links;

public sealed class RecordDomainEvents(FhsCommandDbContext db) : ILink<IRaisesEvents>
{
    public ValueTask<LinkResult> RunAsync(IRaisesEvents state, CancellationToken ct)
    {
        db.Set<OutboxMessage>()
            .AddRange(
                state.Events.Select(e => new OutboxMessage
                {
                    Type = e.GetType().FullName!,
                    Payload = JsonSerializer.Serialize(e, e.GetType()),
                    OccurredAt = e.OccurredAt
                })
            );

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
