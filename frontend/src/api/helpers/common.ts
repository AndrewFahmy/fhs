export function pageOf(search: URLSearchParams): number {
    const page = Number(search.get("page"));

    return Number.isInteger(page) && page > 0 ? page : 1;
}
