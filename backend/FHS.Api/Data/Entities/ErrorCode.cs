using FHS.Api.Enums;
using FHS.Api.Interfaces;

namespace FHS.Api.Data.Entities;

public sealed class ErrorCode : IDbEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Code { get; init; }

    public required string Description { get; set; }

    public required Severity Severity { get; init; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Postgres' system <c>xmin</c> column, used as an optimistic concurrency token.
    /// Maintained by the database; never assigned by the application.
    /// </summary>
    public uint Version { get; private set; }
}
