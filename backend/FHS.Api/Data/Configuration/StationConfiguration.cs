using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public static readonly int CodeMaxLength = AppConstants.Data.CodeMaxLength;
    public static readonly int NameMaxLength = 200;

    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Code).HasMaxLength(CodeMaxLength);
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(NameMaxLength);
    }
}
