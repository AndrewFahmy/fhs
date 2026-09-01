using FHS.Api.Interfaces;

namespace FHS.Api.Data.Entities;

/// <summary>
/// Whoever or whatever performed an action — today always a person mapped from a Keycloak
/// subject, but deliberately not named <c>User</c>: automated inspection stations and system
/// jobs raise defects too, and <c>User</c> already means <see cref="System.Security.Claims.ClaimsPrincipal"/>
/// in this codebase (<c>HttpContext.User</c>, <c>ICurrentUser</c>).
/// </summary>
public sealed class Actor : IDbEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string SubjectId { get; init; }

    public required string DisplayName { get; set; }

    /// <summary>
    /// Postgres' system <c>xmin</c> column, used as an optimistic concurrency token.
    /// Maintained by the database; never assigned by the application.
    /// </summary>
    public uint Version { get; private set; }
}
