namespace FHS.Api.Domains;

public sealed class Station
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Code { get; init; }

    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;
}
