using FHS.Chain.Enums;

namespace FHS.Chain.Primitives;

public sealed record Error(
    string Code,
    string Message,
    ErrorKind Kind,
    IReadOnlyList<FieldError>? Fields = null
)
{
    public static Error Unexpected() =>
        new("Unexpected", "An unexpected error occurred.", ErrorKind.Unexpected);
}
