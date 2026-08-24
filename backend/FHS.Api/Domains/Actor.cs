namespace FHS.Api.Domains;

public sealed class Actor
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string SubjectId { get; init; }

    public required string DisplayName { get; set; }
}
