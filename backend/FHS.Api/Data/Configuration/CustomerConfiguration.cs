using FHS.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Data.Configuration;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Code).HasMaxLength(AppConstants.Data.CustomerCodeMaxLength);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Name).HasMaxLength(AppConstants.Data.CustomerNameMaxLength);

        builder.Property(x => x.Version).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
    }
}
