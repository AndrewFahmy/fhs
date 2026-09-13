using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class DefectConfiguration : IEntityTypeConfiguration<Defect>
{
    public void Configure(EntityTypeBuilder<Defect> builder)
    {
        builder.ToTable("defects");

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
            .HasOne(d => d.Facility)
            .WithMany()
            .HasForeignKey(d => d.FacilityId)
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

        builder
            .HasIndex(d => new
            {
                d.FacilityId,
                d.CreatedAt,
                d.Id
            })
            .IsDescending(false, true, true);

        builder
            .HasIndex(d => new { d.FacilityId, d.CreatedAt })
            .IsDescending(false, true)
            .HasFilter("resolved_at IS NULL");

        builder
            .HasIndex(d => new
            {
                d.FacilityId,
                d.StationId,
                d.CreatedAt
            })
            .IsDescending(false, false, true);

        builder
            .HasIndex(d => new
            {
                d.FacilityId,
                d.ErrorCodeId,
                d.CreatedAt
            })
            .IsDescending(false, false, true);
    }
}
