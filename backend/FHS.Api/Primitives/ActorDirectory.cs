using FHS.Api.Data;
using FHS.Api.Domains;
using FHS.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Primitives;

public sealed class ActorDirectory(FhsCommandDbContext db) : IActorDirectory
{
    public Task<Actor?> FindAsync(string subjectId, CancellationToken ct) =>
        db.Set<Actor>().SingleOrDefaultAsync(a => a.SubjectId == subjectId, ct);
}
