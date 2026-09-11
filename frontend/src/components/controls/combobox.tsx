import { useEffect, useId, useRef, useState } from "react";
import Icon from "@/components/common/icon";

export interface ComboboxOption {
    value: string;
    /** The code, rendered monospaced. */
    label: string;
    /** The human name, rendered muted next to it. */
    hint?: string;
}

export interface ComboboxProps {
    label: string;
    value: string;
    options: ComboboxOption[];
    onChange: (value: string) => void;
    placeholder?: string;
    /** Adds a row that clears the selection, e.g. "All stations". */
    clearLabel?: string;
    className?: string;
    disabled?: boolean;
    required?: boolean;
    error?: string;
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

function textOf(option: ComboboxOption): string {
    return option.hint ? `${option.label} — ${option.hint}` : option.label;
}

function Combobox({
    label,
    value,
    options,
    onChange,
    placeholder,
    clearLabel,
    className = "w-56",
    disabled = false,
    required = false,
    error,
    variant = "filter",
}: ComboboxProps) {
    const [open, setOpen] = useState(false);
    const [query, setQuery] = useState("");
    const [highlight, setHighlight] = useState(0);

    const listId = useId();
    const containerRef = useRef<HTMLDivElement>(null);

    const styles = variantStyles[variant];
    const borderClass = error
        ? "border-severity-critical"
        : "border-border-control";

    const selected = options.find((option) => option.value === value);

    const matches = query
        ? options.filter((option) =>
              textOf(option).toLowerCase().includes(query.toLowerCase()),
          )
        : options;

    const rows: ComboboxOption[] = clearLabel
        ? [{ value: "", label: clearLabel }, ...matches]
        : matches;

    useEffect(() => {
        if (!open) {
            return;
        }

        function closeOnOutsidePointer(event: PointerEvent) {
            if (!containerRef.current?.contains(event.target as Node)) {
                setOpen(false);
                setQuery("");
            }
        }

        document.addEventListener("pointerdown", closeOnOutsidePointer);

        return () =>
            document.removeEventListener("pointerdown", closeOnOutsidePointer);
    }, [open]);

    function choose(option: ComboboxOption) {
        onChange(option.value);
        setQuery("");
        setOpen(false);
    }

    function openList() {
        if (open) {
            return;
        }

        setQuery("");
        setHighlight(0);
        setOpen(true);
    }

    function closeList() {
        setOpen(false);
        setQuery("");
    }

    function onKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === "ArrowDown") {
            event.preventDefault();
            setOpen(true);
            setHighlight((index) => Math.min(index + 1, rows.length - 1));
            return;
        }

        if (event.key === "ArrowUp") {
            event.preventDefault();
            setHighlight((index) => Math.max(index - 1, 0));
            return;
        }

        if (event.key === "Enter" && open && rows[highlight]) {
            event.preventDefault();
            choose(rows[highlight]);
            return;
        }

        if (event.key === "Escape") {
            closeList();
        }
    }

    return (
        <div ref={containerRef} className={`flex flex-col gap-2 ${className}`}>
            <label className={styles.label} htmlFor={`${listId}-input`}>
                {label}
                {required ? (
                    <span className="text-severity-critical"> *</span>
                ) : null}
            </label>

            <div className="relative">
                <input
                    id={`${listId}-input`}
                    role="combobox"
                    aria-expanded={open}
                    aria-controls={listId}
                    aria-autocomplete="list"
                    aria-activedescendant={
                        open ? `${listId}-option-${highlight}` : undefined
                    }
                    autoComplete="off"
                    disabled={disabled}
                    placeholder={placeholder}
                    value={open ? query : selected ? textOf(selected) : ""}
                    onChange={(event) => {
                        setQuery(event.target.value);
                        setHighlight(0);
                        setOpen(true);
                    }}
                    onFocus={openList}
                    onPointerDown={(event) => {
                        // Not focused yet: the focus that follows opens the list.
                        if (document.activeElement !== event.currentTarget) {
                            return;
                        }

                        if (open) {
                            closeList();
                        } else {
                            openList();
                        }
                    }}
                    onKeyDown={onKeyDown}
                    className={`${styles.control} ${borderClass} w-full border bg-surface-input px-3 pr-9 text-sm text-ink placeholder:text-ink-muted focus-visible:border-border-focus focus-visible:outline-none disabled:opacity-60`}
                />
                <Icon
                    name="chevron-down"
                    className="pointer-events-none absolute top-1/2 right-3 size-3 -translate-y-1/2 text-ink"
                />

                {open ? (
                    <ul
                        id={listId}
                        role="listbox"
                        className="absolute top-full right-0 left-0 z-20 mt-1 max-h-64 overflow-y-auto rounded-[5px] border border-border-control bg-surface p-1 shadow-lg"
                    >
                        {rows.length === 0 ? (
                            <li className="px-3 py-2 text-sm text-ink-muted">
                                No matches
                            </li>
                        ) : (
                            rows.map((option, index) => (
                                <li
                                    key={option.value || "clear"}
                                    id={`${listId}-option-${index}`}
                                    role="option"
                                    aria-selected={option.value === value}
                                    onPointerDown={(event) => {
                                        event.preventDefault();
                                        choose(option);
                                    }}
                                    onPointerEnter={() => setHighlight(index)}
                                    className={`flex cursor-pointer items-baseline gap-3 rounded-[3px] px-3 py-2 text-sm ${
                                        index === highlight
                                            ? "bg-surface-head"
                                            : ""
                                    }`}
                                >
                                    <span
                                        className={
                                            option.value
                                                ? "font-mono font-bold text-ink"
                                                : "text-ink-muted"
                                        }
                                    >
                                        {option.label}
                                    </span>
                                    {option.hint ? (
                                        <span className="truncate text-ink-muted">
                                            {option.hint}
                                        </span>
                                    ) : null}
                                </li>
                            ))
                        )}
                    </ul>
                ) : null}
            </div>

            {error ? (
                <span className="text-sm text-severity-critical">{error}</span>
            ) : null}
        </div>
    );
}

export default Combobox;
