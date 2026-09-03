using FHS.Api.Interfaces;

namespace FHS.Api.Data.Entities;

public sealed class Customer : IDbEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Code { get; init; }

    public required string Name { get; init; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Postgres' system <c>xmin</c> column, used as an optimistic concurrency token.
    /// Maintained by the database; never assigned by the application.
    /// </summary>
    public uint Version { get; private set; }
}
