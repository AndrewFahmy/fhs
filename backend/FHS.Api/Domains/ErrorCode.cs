using FHS.Api.Enums;

namespace FHS.Api.Domains;

public sealed class ErrorCode
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Code { get; init; }

    public required string Description { get; set; }

    public required Severity Severity { get; init; }

    public bool IsActive { get; set; } = true;
}
