import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { SetURLSearchParams } from "react-router";
import Select from "@/components/controls/select";
import RadioGroup from "@/components/controls/radio-group";
import type { ErrorCodeListItem, StationListItem } from "@/api/types";
import {
    defectsPageHasFilters,
    parseDefectStatusFilter,
} from "@/api/helpers/defects";
import { Button } from "@/components/controls/button";

const statusOptions = [
    { value: "open", label: "Open" },
    { value: "resolved", label: "Resolved" },
    { value: "all", label: "All" },
];

const severityOptions = [
    { value: "Minor", label: "Minor" },
    { value: "Major", label: "Major" },
    { value: "Critical", label: "Critical" },
];

export interface DefectsFiltersProps {
    search: URLSearchParams;
    setSearch: SetURLSearchParams;
}

function DefectsFilters({ search, setSearch }: DefectsFiltersProps) {
    const stations = useGet<StationListItem[]>(endpoints.getStations);
    const errorCodes = useGet<ErrorCodeListItem[]>(endpoints.getErrorCodes);

    function setFilter(key: string, value: string) {
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

    return (
        <div className="mt-6 flex flex-wrap items-end gap-6 rounded-[5px] border border-border-subtle bg-surface-subtle px-6 py-5">
            <Select
                label="Station"
                placeholder="All stations"
                disabled={stations.loading}
                value={search.get("stationCode") ?? ""}
                options={(stations.data ?? []).map((station) => ({
                    value: station.code,
                    label: `${station.code} — ${station.name}`,
                }))}
                onChange={(value) => setFilter("stationCode", value)}
            />

            <Select
                label="Error code"
                placeholder="All error codes"
                disabled={errorCodes.loading}
                value={search.get("errorCode") ?? ""}
                options={(errorCodes.data ?? []).map((errorCode) => ({
                    value: errorCode.code,
                    label: `${errorCode.code} — ${errorCode.description}`,
                }))}
                onChange={(value) => setFilter("errorCode", value)}
            />

            <Select
                label="Severity"
                className="w-44"
                placeholder="All severities"
                value={search.get("severity") ?? ""}
                options={severityOptions}
                onChange={(value) => setFilter("severity", value)}
            />

            <RadioGroup
                label="Status"
                name="status"
                value={parseDefectStatusFilter(search)}
                options={statusOptions}
                onChange={(value) =>
                    setFilter("status", value === "open" ? "" : value)
                }
            />

            <Button
                variant="ghost"
                size="sm"
                className="ml-auto font-bold hover:underline"
                disabled={!defectsPageHasFilters(search)}
                onClick={() => setSearch(new URLSearchParams())}
            >
                Clear
            </Button>
        </div>
    );
}

export default DefectsFilters;
