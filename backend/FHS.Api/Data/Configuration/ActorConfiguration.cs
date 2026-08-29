using FHS.Api.Data.Entities;
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

        builder.Property(x => x.Version).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder.HasData(
            new Actor
            {
                Id = AppConstants.Data.LineOperatorActorId,
                SubjectId = AppConstants.Data.LineOperatorSubjectId,
                DisplayName = "Line Operator"
            },
            new Actor
            {
                Id = AppConstants.Data.AdminActorId,
                SubjectId = AppConstants.Data.AdminSubjectId,
                DisplayName = "System Administrator",
            }
        );
    }
}
