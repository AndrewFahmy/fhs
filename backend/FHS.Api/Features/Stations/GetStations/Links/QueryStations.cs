using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace FHS.Api.Features.Stations;

public sealed class QueryStation(FhsQueryDbContext db) : ILink<GetStationsState>
{
    public async ValueTask<LinkResult> RunAsync(GetStationsState state, CancellationToken ct)
    {
        IQueryable<Station> stations = db.Set<Station>();

        if (!state.IncludeInactive)
        {
            stations = stations.Where(s => s.IsActive);
        }

        var items = await stations
            .OrderBy(s => s.Code)
            .Select(s => new StationListItem(s.Id, s.Code, s.Name, s.IsActive))
            .ToListAsync(ct);

        state.Produce(items);

        return LinkResult.Continue;
    }
}
