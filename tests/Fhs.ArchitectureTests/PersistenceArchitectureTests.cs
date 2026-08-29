using FHS.Api.Data;
using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fhs.ArchitectureTests;

public sealed class PersistenceArchitectureTests
{
    [Fact]
    public void Every_entity_except_the_outbox_has_a_concurrency_token()
    {
        var options = new DbContextOptionsBuilder<FhsCommandDbContext>()
            .UseNpgsql("Host=localhost;Database=fhs-db")
            .UseSnakeCaseNamingConvention()
            .Options;

        using var db = new FhsCommandDbContext(options);

        var missing = db
            .Model.GetEntityTypes()
            .Where(e => e.ClrType != typeof(OutboxMessage))
            .Where(e => !e.GetProperties().Any(p => p.IsConcurrencyToken))
            .Select(e => e.ClrType.Name)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"""
            No optimistic concurrency token on: {string.Join(", ", missing)}.
            Map xmin with .HasColumnName("xmin").HasColumnType("xmin").IsRowVersion(),
            or exclude the type here with a reason.
            """
        );
    }
}
