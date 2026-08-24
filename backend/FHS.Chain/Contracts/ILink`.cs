using FHS.Chain.Primitives;

namespace FHS.Chain.Contracts;

public interface ILink<in TState>
{
    ValueTask<LinkResult> RunAsync(TState state, CancellationToken ct);
}