using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class EscapeConfiguration : IEntityTypeConfiguration<Escape>
{
    public void Configure(EntityTypeBuilder<Escape> builder)
    {
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
            .HasOne(e => e.Reporter)
            .WithMany()
            .HasForeignKey(e => e.ReportedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Resolver)
            .WithMany()
            .HasForeignKey(e => e.ResolvedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
