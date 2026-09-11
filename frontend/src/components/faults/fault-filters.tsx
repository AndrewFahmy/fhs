import type { ReactNode } from "react";
import { Button } from "@/components/controls/button";
import RadioGroup from "@/components/controls/radio-group";
import Select from "@/components/controls/select";
import ErrorCodeCombobox from "@/components/errorCodes/error-code-combobox";
import type { FaultFilterState } from "@/utils/use-fault-filters";

const severityOptions = [
    { value: "Minor", label: "Minor" },
    { value: "Major", label: "Major" },
    { value: "Critical", label: "Critical" },
];

const statusOptions = [
    { value: "open", label: "Open" },
    { value: "resolved", label: "Resolved" },
    { value: "all", label: "All" },
];

export interface FaultFiltersProps {
    filters: FaultFilterState;
    /** The source picker: Station for defects, Customer for escapes. */
    children: ReactNode;
}

function FaultFilters({ filters, children }: FaultFiltersProps) {
    return (
        <div className="mt-6 flex flex-wrap items-end gap-6 rounded-[5px] border border-border-subtle bg-surface-subtle px-6 py-5">
            {children}

            <ErrorCodeCombobox
                placeholder="All error codes"
                clearLabel="All error codes"
                value={filters.get("errorCode")}
                onChange={(code) => filters.set("errorCode", code)}
            />

            <Select
                label="Severity"
                className="w-44"
                placeholder="All severities"
                value={filters.get("severity")}
                options={severityOptions}
                onChange={(value) => filters.set("severity", value)}
            />

            <RadioGroup
                label="Status"
                name="status"
                value={filters.status}
                options={statusOptions}
                onChange={(value) =>
                    filters.set("status", value === "open" ? "" : value)
                }
            />

            <Button
                variant="ghost"
                size="sm"
                className="ml-auto font-bold hover:underline"
                disabled={!filters.filtered}
                onClick={filters.clear}
            >
                Clear
            </Button>
        </div>
    );
}

export default FaultFilters;
