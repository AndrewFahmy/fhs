using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class DefectConfiguration : IEntityTypeConfiguration<Defect>
{
    public static readonly int DescriptionMaxLength = AppConstants.Data.DescriptionMaxLength;
    public static readonly int ResolutionMaxLength = 500;
    public static readonly int SeverityMaxLength = AppConstants.Data.SeverityMaxLength;

    public void Configure(EntityTypeBuilder<Defect> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Description).HasMaxLength(DescriptionMaxLength);
        builder.Property(d => d.Resolution).HasMaxLength(ResolutionMaxLength);

        builder.Property(d => d.Severity).HasConversion<string>().HasMaxLength(SeverityMaxLength);

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
    }
}
