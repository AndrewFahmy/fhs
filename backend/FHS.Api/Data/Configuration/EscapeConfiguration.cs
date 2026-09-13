using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class EscapeConfiguration : IEntityTypeConfiguration<Escape>
{
    public void Configure(EntityTypeBuilder<Escape> builder)
    {
        builder.ToTable(
            "escapes",
            t =>
                t.HasCheckConstraint(
                    "ck_escapes_attribution_complete",
                    "(attributed_defect_id IS NULL) = (attribution_basis IS NULL)"
                )
        );

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Description).HasMaxLength(AppConstants.Data.EscapeDescriptionMaxLength);
        builder.Property(e => e.Resolution).HasMaxLength(AppConstants.Data.EscapeResolutionMaxLength);

        builder.Property(e => e.Version).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder
            .Property(e => e.Severity)
            .HasConversion<string>()
            .HasMaxLength(AppConstants.Data.EscapeSeverityMaxLength);

        builder
            .Property(e => e.AttributionBasis)
            .HasConversion<string>()
            .HasMaxLength(AppConstants.Data.EscapeAttributionBasisMaxLength);

        builder
            .HasOne(e => e.AttributedDefect)
            .WithMany()
            .HasForeignKey(e => e.AttributedDefectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.ErrorCode)
            .WithMany()
            .HasForeignKey(e => e.ErrorCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Facility)
            .WithMany()
            .HasForeignKey(e => e.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Reporter)
            .WithMany()
            .HasForeignKey(e => e.ReportedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Resolver)
            .WithMany()
            .HasForeignKey(e => e.ResolvedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(e => new
            {
                e.FacilityId,
                e.ReportedAt,
                e.Id
            })
            .IsDescending(false, true, true);

        builder
            .HasIndex(e => new { e.FacilityId, e.ReportedAt })
            .IsDescending(false, true)
            .HasFilter("resolved_at IS NULL");

        builder
            .HasIndex(e => new
            {
                e.FacilityId,
                e.CustomerId,
                e.ReportedAt
            })
            .IsDescending(false, false, true);

        builder
            .HasIndex(e => new
            {
                e.FacilityId,
                e.ErrorCodeId,
                e.ReportedAt
            })
            .IsDescending(false, false, true);

        builder.HasIndex(e => e.AttributedDefectId).HasFilter("attributed_defect_id IS NOT NULL");
    }
}
