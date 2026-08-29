using FHS.Api.Data.Entities;

namespace FHS.Api.Interfaces;

public interface IHasActor
{
    Actor Actor { get; set; }
}
