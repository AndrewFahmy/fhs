using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fhs.IntegrationTests.Extensions;

internal static class AssertionExtensions
{
    extension(FhsApiFactory factory)
    {
        public Task<TEntity?> FindAsync<TEntity>(Guid id, CancellationToken ct) where TEntity: class, IDbEntity =>
            factory.QueryAsync(
                (db, token) => db.Set<TEntity>().AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, token), 
                ct
            );


        public async Task<IReadOnlyList<OutboxMessage>> OutboxForAsync(Guid subjectId, CancellationToken ct)
        {
            var all = await factory.QueryAsync(
                (db, token) => db.Set<OutboxMessage>().AsNoTracking().OrderBy(m => m.Id).ToListAsync(token),
                ct
            );

            return [.. all.Where(m => m.Payload.Contains(subjectId.ToString(), StringComparison.OrdinalIgnoreCase))];
        }


        // Private Methods
        private async Task<T> QueryAsync<T>(
            Func<FhsCommandDbContext, CancellationToken, Task<T>> query,
            CancellationToken ct
        )
        {
            await using var scope = factory.Services.CreateAsyncScope();

            return await query(scope.ServiceProvider.GetRequiredService<FhsCommandDbContext>(), ct);
        }
    }
}