using FHS.Chain.Enums;

namespace FHS.Chain.Primitives;

public readonly record struct LinkResult
{
    private LinkResult(LinkOutcome kind, Error? error)
    {
        Kind = kind;
        Error = error;
    }

    public LinkOutcome Kind { get; }
    public Error? Error { get; }

    public static LinkResult Continue { get; } = new(LinkOutcome.Continue, null);
    public static LinkResult Done { get; } = new(LinkOutcome.Done, null);

    public static LinkResult Fail(Error error) => new(LinkOutcome.Fail, error);
}
