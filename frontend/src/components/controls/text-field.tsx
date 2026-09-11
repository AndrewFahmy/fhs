export interface TextFieldProps {
    label: string;
    value: string;
    onChange: (value: string) => void;
    maxLength?: number;
    placeholder?: string;
    error?: string;
    disabled?: boolean;
    required?: boolean;
}

function TextField({
    label,
    value,
    onChange,
    maxLength,
    placeholder,
    error,
    disabled = false,
    required = false,
}: TextFieldProps) {
    const borderClass = error
        ? "border-severity-critical"
        : "border-border-control";

    return (
        <label className="flex flex-col gap-2">
            <span className="text-sm font-bold text-ink">
                {label}
                {required ? (
                    <span className="text-severity-critical"> *</span>
                ) : null}
            </span>
            <input
                type="text"
                value={value}
                maxLength={maxLength}
                disabled={disabled}
                placeholder={placeholder}
                onChange={(event) => onChange(event.target.value)}
                className={`h-12 w-full rounded-sm border ${borderClass} bg-surface-input px-3 text-sm text-ink placeholder:text-ink-muted focus-visible:border-border-focus focus-visible:outline-none disabled:opacity-60`}
            />
            {error ? (
                <span className="text-sm text-severity-critical">{error}</span>
            ) : null}
        </label>
    );
}

export default TextField;
