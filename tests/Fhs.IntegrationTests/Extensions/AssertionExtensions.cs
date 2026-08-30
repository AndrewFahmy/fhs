using FHS.Api.Data;
using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fhs.IntegrationTests.Extensions;

internal static class AssertionExtensions
{
    extension(FhsApiFactory factory)
    {
        public Task<Defect?> FindDefectAsync(Guid id, CancellationToken ct) =>
            factory.QueryAsync(
                (db, token) => db.Set<Defect>().AsNoTracking().SingleOrDefaultAsync(d => d.Id == id, token),
                ct
            );


        public async Task<IReadOnlyList<OutboxMessage>> OutboxForAsync(Guid subjectId, CancellationToken ct)
        {
            var all = await factory.QueryAsync(
                (db, token) => db.Set<OutboxMessage>().AsNoTracking().ToListAsync(token),
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