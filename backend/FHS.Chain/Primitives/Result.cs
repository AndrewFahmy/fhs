using System.Diagnostics.CodeAnalysis;

namespace FHS.Chain.Primitives;

public readonly record struct Result
{
    private readonly Error? _error;

    private Result(Error error) => _error = error;

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => _error is null;

    public Error Error =>
        _error ?? throw new InvalidOperationException("Result succeeded; there is no error to read.");

    public static Result Success => default;

    public static Result Fail(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }

    public override string ToString() => _error is null ? "Success" : $"Fail({_error.Code})";
}
