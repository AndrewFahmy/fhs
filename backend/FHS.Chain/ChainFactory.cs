using FHS.Chain.Builder;
using FHS.Chain.Contracts;

namespace FHS.Chain;

public static class Chain
{
    public static ChainBuilder<TState> For<TState>()
        where TState : ChainState => new();

    public static ChainBuilder<TState, TResult> For<TState, TResult>()
        where TState : ChainState<TResult> => new();
}
