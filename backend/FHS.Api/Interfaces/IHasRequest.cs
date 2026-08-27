namespace FHS.Api.Interfaces;

public interface IHasRequest<out TRequest>
{
    TRequest Request { get; }
}
