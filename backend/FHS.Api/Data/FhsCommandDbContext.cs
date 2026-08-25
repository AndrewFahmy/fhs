using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Data;

public sealed class FhsCommandDbContext : DbContext
{
    public FhsCommandDbContext(DbContextOptions<FhsCommandDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FhsCommandDbContext).Assembly);
}
