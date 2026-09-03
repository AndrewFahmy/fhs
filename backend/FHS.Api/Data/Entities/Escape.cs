using FHS.Api.Enums;
using FHS.Api.Interfaces;

namespace FHS.Api.Data.Entities;

public sealed class Escape : IDbEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required Guid CustomerId { get; init; }

    public required Guid ErrorCodeId { get; init; }

    public required string Description { get; init; }

    public required Severity Severity { get; init; }

    public required Guid ReportedBy { get; init; }

    public required DateTimeOffset ReportedAt { get; init; }

    public string? Resolution { get; set; }

    public Guid? ResolvedBy { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    /// <summary>
    /// Postgres' system <c>xmin</c> column, used as an optimistic concurrency token.
    /// Maintained by the database; never assigned by the application.
    /// </summary>
    public uint Version { get; private set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;

    public ErrorCode ErrorCode { get; set; } = null!;

    public Actor Reporter { get; set; } = null!;

    public Actor? Resolver { get; set; }
}
