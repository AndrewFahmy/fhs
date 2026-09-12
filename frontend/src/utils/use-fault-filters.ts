import { useSearchParams } from "react-router";
import { settings } from "@/api/api-constants";
import type { StatusFilter } from "@/api/enums";
import { pageOf } from "@/api/api-helpers";

function statusOf(search: URLSearchParams): StatusFilter {
    const status = search.get("status");

    return status === "resolved" || status === "all" ? status : "open";
}

/**
 * URL-backed filters for the paged fault lists (defects and escapes).
 * Only the source differs: a defect's station, an escape's customer.
 */
export function useFaultFilters(sourceKey: "stationCode" | "customerCode") {
    const [search, setSearch] = useSearchParams();

    const keys = [sourceKey, "errorCode", "severity"];
    const status = statusOf(search);

    const query = new URLSearchParams();

    for (const key of [...keys, "page"]) {
        const value = search.get(key);

        if (value) {
            query.set(key, value);
        }
    }

    query.set("pageSize", String(settings.defaultPageSize));

    if (status !== "all") {
        query.set("isResolved", String(status === "resolved"));
    }

    /** Changing any filter returns to page 1. */
    function set(key: string, value: string) {
        setSearch((previous) => {
            const params = new URLSearchParams(previous);

            if (value) {
                params.set(key, value);
            } else {
                params.delete(key);
            }

            params.delete("page");

            return params;
        });
    }

    function setPage(page: number) {
        setSearch((previous) => {
            const params = new URLSearchParams(previous);
            params.set("page", String(page));

            return params;
        });
    }

    function clear() {
        setSearch(new URLSearchParams());
    }

    return {
        query: query.toString(),
        status,
        page: pageOf(search),
        filtered: status !== "open" || keys.some((key) => search.has(key)),
        /** Filters other than status that are currently applied. */
        activeCount: keys.filter((key) => search.has(key)).length,
        get: (key: string) => search.get(key) ?? "",
        set,
        setPage,
        clear,
    };
}

export type FaultFilterState = ReturnType<typeof useFaultFilters>;
