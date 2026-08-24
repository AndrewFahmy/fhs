using FHS.Chain.Contracts;

namespace FHS.Chain.Builder;

public sealed class ChainBuilder<TState, TResult>
    where TState : ChainState<TResult>
{
    private readonly List<LinkDescriptor> _links = [];

    internal ChainBuilder() { }

    public ChainBuilder<TState, TResult> Link<TLink>()
        where TLink : class, ILink<TState>
    {
        _links.Add(LinkDescriptor.For<TLink>());
        return this;
    }

    public Chain<TState, TResult> Build()
    {
        var name = ChainWiring.NameOf(typeof(TState));
        ChainWiring.Verify(name, _links);

        return new Chain<TState, TResult>(name, [.. _links]);
    }
}
