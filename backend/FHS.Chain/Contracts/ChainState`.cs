using FHS.Chain.Exceptions;

namespace FHS.Chain.Contracts;

public abstract class ChainState<TResult> : ChainState
{
    private TResult _result = default!;

    internal bool HasResult { get; private set; }

    internal TResult Result =>
        HasResult
            ? _result
            : throw new ChainResultException(
                $"""
                {ChainWiring.NameOf(GetType())} completed without producing a {typeof(TResult).Name}
                Either no link called Produce(), or a link returned LinkResult.Done before the producing link ran.
                """
            );

    public void Produce(TResult result)
    {
        _result = result;
        HasResult = true;
    }
}
