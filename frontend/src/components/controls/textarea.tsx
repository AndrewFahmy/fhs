export interface TextareaProps {
    label: string;
    value: string;
    maxLength: number;
    onChange: (value: string) => void;
    placeholder?: string;
    error?: string;
    rows?: number;
    disabled?: boolean;
}

function Textarea({
    label,
    value,
    maxLength,
    onChange,
    placeholder,
    error,
    rows = 5,
    disabled = false,
}: TextareaProps) {
    return (
        <label className="block">
            <span className="flex items-baseline gap-3">
                <span className="text-sm font-bold text-ink">{label}</span>
                <span className="text-xs text-ink-muted">
                    {value.length} / {maxLength}
                </span>
            </span>
            <textarea
                value={value}
                rows={rows}
                maxLength={maxLength}
                disabled={disabled}
                placeholder={placeholder}
                onChange={(event) => onChange(event.target.value)}
                className="mt-3 w-full resize-y rounded-sm border-[1.5px] border-border-control bg-surface-input px-4 py-3 text-sm text-ink placeholder:text-ink-muted focus-visible:border-border-focus focus-visible:outline-none disabled:opacity-60"
            />
            {error ? (
                <span className="mt-2 block text-sm text-severity-critical">
                    {error}
                </span>
            ) : null}
        </label>
    );
}

export default Textarea;
