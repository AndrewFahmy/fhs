using FHS.Api.Domains;

namespace FHS.Api.Interfaces;

public interface IHasActor
{
    Actor Actor { get; set; }
}
