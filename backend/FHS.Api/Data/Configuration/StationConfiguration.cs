using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Code).HasMaxLength(AppConstants.Data.StationCodeMaxLength);
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(AppConstants.Data.StationNameMaxLength);
    }
}
