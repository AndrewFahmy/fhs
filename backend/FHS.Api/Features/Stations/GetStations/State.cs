using FHS.Chain.Contracts;

namespace FHS.Api.Features.Stations;

public sealed class GetStationsState(bool includeInactive) : ChainState<IReadOnlyList<StationListItem>>
{
    public bool IncludeInactive { get; } = includeInactive;
}
