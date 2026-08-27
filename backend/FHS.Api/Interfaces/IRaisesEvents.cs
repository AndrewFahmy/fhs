namespace FHS.Api.Interfaces;

public interface IRaisesEvents
{
    List<IDomainEvent> Events { get; }
}
