import Icon from "@/components/common/icon";

export interface SelectOption {
    value: string;
    label: string;
}

export interface SelectProps {
    label: string;
    value: string;
    options: SelectOption[];
    placeholder?: string;
    onChange: (value: string) => void;
    className?: string;
    disabled?: boolean;
}

function Select({
    label,
    value,
    options,
    placeholder,
    onChange,
    className,
    disabled,
}: SelectProps) {
    return (
        <label className={`flex flex-col gap-2 ${className}`}>
            <span className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                {label}
            </span>
            <span className="relative block">
                <select
                    value={value}
                    disabled={disabled}
                    onChange={(event) => onChange(event.target.value)}
                    className="h-10 w-full appearance-none rounded-[3px] border border-border-control bg-surface-input px-3 pr-9 text-sm text-ink focus-visible:border-border-focus focus-visible:outline-none disabled:opacity-60"
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
        </label>
    );
}

export default Select;
