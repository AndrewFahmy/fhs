using FHS.Chain.Contracts;

namespace FHS.Chain.Builder;

public sealed class ChainBuilder<TState>
    where TState : ChainState
{
    private readonly List<LinkDescriptor> _links = [];

    internal ChainBuilder() { }

    public ChainBuilder<TState> Link<TLink>()
        where TLink : class, ILink<TState>
    {
        _links.Add(LinkDescriptor.For<TLink>());
        return this;
    }

    public Chain<TState> Build()
    {
        var name = ChainWiring.NameOf(typeof(TState));
        ChainWiring.Verify(name, _links);

        return new Chain<TState>(name, [.. _links]);
    }
}
