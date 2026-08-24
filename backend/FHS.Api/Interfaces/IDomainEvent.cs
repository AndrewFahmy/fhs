namespace FHS.Api.Interfaces;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
