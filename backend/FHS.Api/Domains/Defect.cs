using FHS.Api.Enums;

namespace FHS.Api.Domains;

public sealed class Defect
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required Guid StationId { get; init; }
    public required Guid ErrorCodeId { get; init; }
    public required string Description { get; init; }
    public required Severity Severity { get; init; }

    public required Guid CreatedBy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public string? Resolution { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
