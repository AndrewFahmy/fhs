using FHS.Api.Data;
using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

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

    [Fact]
    public void No_migration_creates_the_xmin_system_column()
    {
        var offenders = typeof(FhsCommandDbContext)
            .Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Migration)) && !t.IsAbstract)
            .Select(t => (Migration)Activator.CreateInstance(t)!)
            .SelectMany(m =>
                m.UpOperations.SelectMany(ColumnNames).Select(c => (Migration: m.GetType().Name, Column: c))
            )
            .Where(x => string.Equals(x.Column, "xmin", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Migration)
            .Distinct()
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"""
            {string.Join(", ", offenders)} creates an 'xmin' column. xmin is a Postgres system 
            column that already exists on every row — remove the operation by hand; the model 
            snapshot keeps the mapping.
            """
        );
    }

    private static IEnumerable<string> ColumnNames(MigrationOperation operation) =>
        operation switch
        {
            CreateTableOperation create => create.Columns.Select(c => c.Name),
            AddColumnOperation add => [add.Name],
            _ => [],
        };
}
