using System.Diagnostics.CodeAnalysis;

namespace FHS.Chain.Primitives;

public readonly record struct Result<TValue>
{
    private Result(TValue value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public TValue Value { get; }

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result<TValue> Success(TValue value) => new(value, null);

    public static Result<TValue> Fail(Error error) => new(default!, error);
}
