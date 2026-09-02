using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class DefectConfiguration : IEntityTypeConfiguration<Defect>
{
    public void Configure(EntityTypeBuilder<Defect> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Description).HasMaxLength(AppConstants.Data.DefectDescriptionMaxLength);
        builder.Property(d => d.Resolution).HasMaxLength(AppConstants.Data.DefectResolutionMaxLength);

        builder.Property(x => x.Version).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder
            .Property(d => d.Severity)
            .HasConversion<string>()
            .HasMaxLength(AppConstants.Data.DefectSeverityMaxLength);

        builder
            .HasOne<Station>()
            .WithMany()
            .HasForeignKey(d => d.StationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<ErrorCode>()
            .WithMany()
            .HasForeignKey(d => d.ErrorCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Actor>().WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Actor>().WithMany().HasForeignKey(d => d.ResolvedBy).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(d => d.Station)
            .WithMany()
            .HasForeignKey(d => d.StationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(d => d.ErrorCode)
            .WithMany()
            .HasForeignKey(d => d.ErrorCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(d => d.Creator)
            .WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(d => d.Resolver)
            .WithMany()
            .HasForeignKey(d => d.ResolvedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
