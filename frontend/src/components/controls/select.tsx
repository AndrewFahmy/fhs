import Icon from "@/components/common/icon";

export interface SelectOption {
    value: string;
    label: string;
}

export interface SelectProps {
    label: string;
    value: string;
    options: SelectOption[];
    onChange: (value: string) => void;
    placeholder?: string;
    className?: string;
    disabled?: boolean;
    required?: boolean;
    error?: string;
    /** "filter" for the compact strip, "field" for a form. */
    variant?: "filter" | "field";
}

const variantStyles = {
    filter: {
        label: "text-[11px] font-bold uppercase tracking-wide text-ink-muted",
        control: "h-10 rounded-[3px]",
    },
    field: {
        label: "text-sm font-bold text-ink",
        control: "h-12 rounded-[4px]",
    },
};

function Select({
    label,
    value,
    options,
    onChange,
    placeholder,
    className = "w-56",
    disabled = false,
    required = false,
    error,
    variant = "filter",
}: SelectProps) {
    const styles = variantStyles[variant];
    const borderClass = error
        ? "border-severity-critical"
        : "border-border-control";

    return (
        <label className={`flex flex-col gap-2 ${className}`}>
            <span className={styles.label}>
                {label}
                {required ? (
                    <span className="text-severity-critical"> *</span>
                ) : null}
            </span>
            <span className="relative block">
                <select
                    value={value}
                    disabled={disabled}
                    onChange={(event) => onChange(event.target.value)}
                    className={`${styles.control} ${borderClass} w-full appearance-none border bg-surface-input px-3 pr-9 text-sm text-ink focus-visible:border-border-focus focus-visible:outline-none disabled:opacity-60`}
                >
                    <option value="">{placeholder}</option>
                    {options.map((option) => (
                        <option key={option.value} value={option.value}>
                            {option.label}
                        </option>
                    ))}
                </select>
                <Icon
                    name="chevron-down"
                    className="pointer-events-none absolute top-1/2 right-3 size-3 -translate-y-1/2 text-ink"
                />
            </span>
            {error ? (
                <span className="text-sm text-severity-critical">{error}</span>
            ) : null}
        </label>
    );
}

export default Select;
