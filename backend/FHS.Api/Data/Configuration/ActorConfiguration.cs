using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public static readonly int SubjectMaxLength = 255;
    public static readonly int DisplayNameMaxLength = 200;

    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.SubjectId).HasMaxLength(SubjectMaxLength);
        builder.HasIndex(a => a.SubjectId).IsUnique();

        builder.Property(a => a.DisplayName).HasMaxLength(DisplayNameMaxLength);
    }
}
