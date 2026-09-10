export interface RadioOption {
    value: string;
    label: string;
}

export interface RadioGroupProps {
    label: string;
    name: string;
    value: string;
    options: RadioOption[];
    onChange: (value: string) => void;
}

function RadioGroup({
    label,
    name,
    value,
    options,
    onChange,
}: RadioGroupProps) {
    return (
        <div
            role="radiogroup"
            aria-label={label}
            className="flex flex-col gap-2"
        >
            <span className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                {label}
            </span>
            <div className="flex h-10 items-center gap-6">
                {options.map((option) => {
                    const selected = option.value === value;

                    return (
                        <label
                            key={option.value}
                            className="flex cursor-pointer items-center gap-2.5"
                        >
                            <input
                                type="radio"
                                name={name}
                                value={option.value}
                                checked={selected}
                                onChange={() => onChange(option.value)}
                                className="peer sr-only"
                            />
                            <span
                                className={`flex size-3.5 items-center justify-center rounded-full peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-border-focus ${selected ? "bg-action" : "border-[1.3px] border-ink-muted"}`}
                            >
                                {selected ? (
                                    <span className="size-1 rounded-full bg-action-ink" />
                                ) : null}
                            </span>
                            <span
                                className={`text-sm ${selected ? "font-bold text-ink" : "text-ink-muted"}`}
                            >
                                {option.label}
                            </span>
                        </label>
                    );
                })}
            </div>
        </div>
    );
}

export default RadioGroup;
