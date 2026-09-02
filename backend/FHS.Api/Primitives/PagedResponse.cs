namespace FHS.Api.Primitives;

public sealed record PagedResponse<TItem>(IReadOnlyList<TItem> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResponse<TItem> From(IReadOnlyList<TItem> items, Paging paging, int totalCount) =>
        new(items, paging.Number, paging.Size, totalCount);
}
