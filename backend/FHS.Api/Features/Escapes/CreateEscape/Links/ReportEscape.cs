using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Escapes.Links;

[Requires(
    nameof(CreateEscapeState.Actor),
    nameof(CreateEscapeState.Customer),
    nameof(CreateEscapeState.Classification)
)]
public sealed class ReportEscape(FhsCommandDbContext db, TimeProvider clock) : ILink<CreateEscapeState>
{
    public ValueTask<LinkResult> RunAsync(CreateEscapeState state, CancellationToken ct)
    {
        var classification = state.Classification!;
        var now = clock.GetUtcNow();

        Escape escape =
            new()
            {
                CustomerId = state.Customer!.Id,
                ErrorCodeId = classification.ErrorCodeId,
                Severity = classification.Severity,
                Description = state.Request.Description,
                ReportedBy = state.Actor.Id,
                ReportedAt = now
            };

        db.Set<Escape>().Add(escape);

        state.Events.Add(
            new EscapeReported(escape.Id, escape.CustomerId, escape.ErrorCodeId, escape.Severity, now)
        );

        state.Produce(new(escape.Id));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
