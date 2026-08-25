using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class ErrorCodeConfiguration : IEntityTypeConfiguration<ErrorCode>
{
    public static readonly int CodeMaxLength = AppConstants.Data.CodeMaxLength;
    public static readonly int DescriptionMaxLength = AppConstants.Data.DescriptionMaxLength;
    public static readonly int SeverityMaxLength = AppConstants.Data.SeverityMaxLength;

    public void Configure(EntityTypeBuilder<ErrorCode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Code).HasMaxLength(CodeMaxLength);
        builder.HasIndex(e => e.Code).IsUnique();

        builder.Property(e => e.Description).HasMaxLength(DescriptionMaxLength);

        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(SeverityMaxLength);
    }
}
