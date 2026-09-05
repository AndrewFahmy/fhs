using FHS.Chain.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace FHS.Api.Primitives;

public sealed class FhsProblemDetails : ProblemDetails
{
    /// <summary>
    /// The stable error identifier a client branches on — "Escapes.CustomerInactive", not a bare 409.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Populated only on validation failures, where every field error is reported at once.
    /// </summary>
    public IReadOnlyList<FieldError>? Errors { get; init; }

    /// <summary>
    /// The JSON path that could not be read, on a malformed-body rejection. Safe to return —
    /// it names the caller's own member, where the exception message would name internal types.
    /// </summary>
    public string? Path { get; init; }
}
