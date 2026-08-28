using FHS.Api.Data;
using FHS.Api.Domains;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Defects.Links;

[Requires(
    nameof(CreateDefectState.Actor),
    nameof(CreateDefectState.Station),
    nameof(CreateDefectState.Classification)
)]
public sealed class RaiseDefect(FhsCommandDbContext db, TimeProvider clock) : ILink<CreateDefectState>
{
    public ValueTask<LinkResult> RunAsync(CreateDefectState state, CancellationToken ct)
    {
        var classification = state.Classification!;
        var now = clock.GetUtcNow();

        Defect defect =
            new()
            {
                StationId = state.Station!.Id,
                ErrorCodeId = classification.ErrorCodeId,
                Severity = classification.Severity,
                Description = state.Request.Description,
                CreatedBy = state.Actor.Id,
                CreatedAt = now
            };

        db.Set<Defect>().Add(defect);

        state.Events.Add(
            new DefectRaised(defect.Id, defect.StationId, defect.ErrorCodeId, defect.Severity, now)
        );

        state.Produce(new(defect.Id));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
