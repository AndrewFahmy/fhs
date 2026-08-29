using FHS.Api.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHS.Api.Data.Configuration;

public sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.SubjectId).HasMaxLength(AppConstants.Data.ActorSubjectIdMaxLength);
        builder.HasIndex(a => a.SubjectId).IsUnique();

        builder.Property(a => a.DisplayName).HasMaxLength(AppConstants.Data.ActorDisplayNameMaxLength);

        builder.HasData(
            new Actor
            {
                Id = Guid.Parse("a5a5a5a5-0000-4000-8000-000000000001"),
                SubjectId = "11111111-1111-4111-8111-111111111111",
                DisplayName = "Line Operator"
            },
            new Actor
            {
                Id = Guid.Parse("a5a5a5a5-0000-4000-8000-000000000002"),
                SubjectId = "22222222-2222-4222-8222-222222222222",
                DisplayName = "System Administrator",
            }
        );
    }
}
