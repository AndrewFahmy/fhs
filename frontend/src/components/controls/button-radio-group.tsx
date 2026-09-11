import type { RadioOption } from "@/components/controls/radio-group";

export interface ButtonRadioGroupProps {
    /** Accessible name; the control has no visible label. */
    label: string;
    name: string;
    value: string;
    options: RadioOption[];
    onChange: (value: string) => void;
}

function ButtonRadioGroup({
    label,
    name,
    value,
    options,
    onChange,
}: ButtonRadioGroupProps) {
    return (
        <div
            role="radiogroup"
            aria-label={label}
            className="inline-flex h-10.5 rounded-sm border border-border-strong bg-surface-head p-0.5"
        >
            {options.map((option) => {
                const selected = option.value === value;

                return (
                    <label key={option.value} className="flex cursor-pointer">
                        <input
                            type="radio"
                            name={name}
                            value={option.value}
                            checked={selected}
                            onChange={() => onChange(option.value)}
                            className="peer sr-only"
                        />
                        <span
                            className={`flex min-w-28 items-center justify-center rounded-sm px-4 text-[13px] select-none peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-border-focus ${selected ? "bg-action font-bold text-action-ink" : "text-ink-head hover:text-ink"}`}
                        >
                            {option.label}
                        </span>
                    </label>
                );
            })}
        </div>
    );
}

export default ButtonRadioGroup;
