using FHS.Api.Domains;

namespace FHS.Api.Interfaces;

public interface IActorDirectory
{
    Task<Actor?> FindAsync(string subjectId, CancellationToken ct);
}
