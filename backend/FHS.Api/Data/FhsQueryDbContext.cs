using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Data;

public sealed class FhsQueryDbContext : DbContext
{
    public FhsQueryDbContext(DbContextOptions<FhsQueryDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FhsQueryDbContext).Assembly);
    }
}
