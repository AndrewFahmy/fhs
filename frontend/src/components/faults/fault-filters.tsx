import { useState, type ReactNode } from "react";
import Icon from "@/components/common/icon";
import { Button } from "@/components/controls/button";
import RadioGroup from "@/components/controls/radio-group";
import Select from "@/components/controls/select";
import ErrorCodeCombobox from "@/components/lookups/error-code-combobox";
import type { FaultFilterState } from "@/utils/use-fault-filters";
import { severityOptions } from "@/api/enums";

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
    const [open, setOpen] = useState(false);

    const statusLabel =
        statusOptions.find((option) => option.value === filters.status)
            ?.label ?? "Open";

    const summary =
        filters.activeCount > 0
            ? `${statusLabel} · ${filters.activeCount} filter${filters.activeCount === 1 ? "" : "s"}`
            : statusLabel;

    return (
        <>
            <Button
                variant="subtle"
                size="none"
                aria-expanded={open}
                className="mt-6 h-10 w-full px-4 md:hidden"
                onClick={() => setOpen((wasOpen) => !wasOpen)}
            >
                <span className="flex w-full items-center gap-3">
                    <Icon name="menu" className="size-4 shrink-0" />
                    <span className="font-bold">Filters</span>
                    <span className="ml-auto truncate text-ink-muted">
                        {summary}
                    </span>
                    <Icon
                        name="chevron-down"
                        className={`size-3 shrink-0 transition-transform ${open ? "rotate-180" : ""}`}
                    />
                </span>
            </Button>

            <div
                className={`${open ? "flex" : "hidden"} mt-3 flex-col gap-5 rounded-[5px] border border-border-subtle bg-surface-subtle px-5 py-5 md:mt-6 md:flex md:flex-row md:flex-wrap md:items-end md:gap-6 md:px-6`}
            >
                {children}

                <ErrorCodeCombobox
                    className="w-full md:w-56"
                    placeholder="All error codes"
                    clearLabel="All error codes"
                    value={filters.get("errorCode")}
                    onChange={(code) => filters.set("errorCode", code)}
                />

                <Select
                    label="Severity"
                    className="w-full md:w-44"
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
                    variant="link"
                    size="sm"
                    className="ml-auto"
                    disabled={!filters.filtered}
                    onClick={filters.clear}
                >
                    Clear
                </Button>
            </div>
        </>
    );
}

export default FaultFilters;
