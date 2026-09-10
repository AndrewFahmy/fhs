import type { SetURLSearchParams } from "react-router";
import Select from "@/components/controls/select";
import RadioGroup from "@/components/controls/radio-group";
import StationCombobox from "@/components/stations/station-combobox";
import ErrorCodeCombobox from "@/components/errorCodes/error-code-combobox";
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
            <StationCombobox
                placeholder="All stations"
                clearLabel="All stations"
                value={search.get("stationCode") ?? ""}
                onChange={(code) => setFilter("stationCode", code)}
            />

            <ErrorCodeCombobox
                placeholder="All error codes"
                clearLabel="All error codes"
                value={search.get("errorCode") ?? ""}
                onChange={(code) => setFilter("errorCode", code)}
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
