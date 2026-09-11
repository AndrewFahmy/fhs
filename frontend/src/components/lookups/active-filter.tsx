import ButtonRadioGroup from "@/components/controls/button-radio-group";

const options = [
    { value: "active", label: "Active" },
    { value: "all", label: "All" },
];

export interface ActiveFilterProps {
    showAll: boolean;
    onChange: (showAll: boolean) => void;
    /** Rows currently shown; leave undefined while loading. */
    count?: number;
    /** Singular noun for the count, e.g. "station". */
    noun: string;
}

function ActiveFilter({ showAll, onChange, count, noun }: ActiveFilterProps) {
    return (
        <div className="mt-8 flex flex-wrap items-center gap-6">
            <ButtonRadioGroup
                label={`Show ${noun}s`}
                name="show"
                value={showAll ? "all" : "active"}
                options={options}
                onChange={(value) => onChange(value === "all")}
            />
            {count === undefined ? null : (
                <span className="text-[13px] text-ink-muted">
                    {count} {showAll ? "" : "active "}
                    {count === 1 ? noun : `${noun}s`}
                </span>
            )}
        </div>
    );
}

export default ActiveFilter;
