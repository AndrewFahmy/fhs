using FHS.Api.Interfaces;
using FHS.Api.Primitives;
using FHS.Chain.Attributes;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Links;

[Produces(nameof(IHasActor.Actor))]
public sealed class ResolveActor(ICurrentUser user, IActorDirectory directory) : ILink<IHasActor>
{
    public async ValueTask<LinkResult> RunAsync(IHasActor state, CancellationToken ct)
    {
        if (user.SubjectId is not { Length: > 0 } subjectId)
        {
            return LinkResult.Fail(Errors.Unauthenticated());
        }

        var actor = await directory.FindAsync(subjectId, ct);

        if (actor is null)
        {
            return LinkResult.Fail(Errors.UnknownActor(subjectId));
        }

        state.Actor = actor;

        return LinkResult.Continue;
    }
}
