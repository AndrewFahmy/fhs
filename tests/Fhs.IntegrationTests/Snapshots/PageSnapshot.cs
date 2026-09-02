using FHS.Api.Primitives;

namespace Fhs.IntegrationTests.Snapshots;

internal sealed record PageSnapshot(int Page, int PageSize, int TotalCount, int TotalPages)
{
    public static PageSnapshot From<TItem>(PagedResponse<TItem> response) =>
        new(response.Page, response.PageSize, response.TotalCount, response.TotalPages);
}
