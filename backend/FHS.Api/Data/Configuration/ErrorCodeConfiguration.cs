using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class ErrorCodeConfiguration : IEntityTypeConfiguration<ErrorCode>
{
    public void Configure(EntityTypeBuilder<ErrorCode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Code).HasMaxLength(AppConstants.Data.ErrorCodeMaxLength);
        builder.HasIndex(e => e.Code).IsUnique();

        builder.Property(e => e.Description).HasMaxLength(AppConstants.Data.ErrorCodeDescriptionMaxLength);

        builder
            .Property(e => e.Severity)
            .HasConversion<string>()
            .HasMaxLength(AppConstants.Data.ErrorCodeSeverityMaxLength);
    }
}
