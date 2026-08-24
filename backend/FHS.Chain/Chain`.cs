using FHS.Chain.Contracts;

namespace FHS.Chain;

public class Chain<TState>
    where TState : ChainState
{
    internal Chain(string name, IReadOnlyList<LinkDescriptor> links)
    {
        Name = name;
        Links = links;
    }

    public string Name { get; }
    public IReadOnlyList<LinkDescriptor> Links { get; }

    public IReadOnlyList<string> Describe() => [.. Links.Select(l => l.Name)];
}

public sealed class Chain<TState, TResult> : Chain<TState>
    where TState : ChainState<TResult>
{
    internal Chain(string name, IReadOnlyList<LinkDescriptor> links)
        : base(name, links) { }
}
