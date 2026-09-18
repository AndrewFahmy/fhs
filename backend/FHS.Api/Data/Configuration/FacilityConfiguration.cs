using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("facilities");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.Code).HasMaxLength(AppConstants.Data.FacilityCodeMaxLength);
        builder.HasIndex(f => f.Code).IsUnique();

        builder.Property(f => f.Name).HasMaxLength(AppConstants.Data.FacilityNameMaxLength);

        builder.Property(f => f.Version).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder.HasData(
            new Facility
            {
                Id = AppConstants.Data.DefaultFacilityId,
                Code = AppConstants.Data.DefaultFacilityCode,
                Name = "Default Facility"
            }
        );
    }
}
