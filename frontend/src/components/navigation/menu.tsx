import { useEffect, useRef, useState, type ReactNode } from "react";
import { NavLink } from "react-router";
import Icon from "@/components/common/icon";
import { Button } from "@/components/controls/button";
import type { ButtonSize, ButtonVariant } from "@/utils/button-helpers";

export interface MenuProps {
    label: ReactNode;
    align?: "start" | "end";
    chevron?: boolean;
    triggerClassName?: string;
    children: ReactNode;
    triggerVariant?: ButtonVariant;
    triggerSize?: ButtonSize;
}

export function Menu({
    label,
    align = "start",
    chevron = true,
    triggerClassName = "",
    children,
    triggerVariant = "ghost",
    triggerSize = "none",
}: MenuProps) {
    const [open, setOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!open) return;

        function closeOnOutsidePointer(event: PointerEvent) {
            if (!containerRef.current?.contains(event.target as Node)) {
                setOpen(false);
            }
        }

        function closeOnEscape(event: KeyboardEvent) {
            if (event.key === "Escape") {
                setOpen(false);
            }
        }

        document.addEventListener("pointerdown", closeOnOutsidePointer);
        document.addEventListener("keydown", closeOnEscape);

        return () => {
            document.removeEventListener("pointerdown", closeOnOutsidePointer);
            document.removeEventListener("keydown", closeOnEscape);
        };
    }, [open]);

    return (
        <div ref={containerRef} className="relative">
            <Button
                variant={triggerVariant}
                size={triggerSize}
                className={triggerClassName}
                aria-haspopup="true"
                aria-expanded={open}
                onClick={() => setOpen((wasOpen) => !wasOpen)}
            >
                {label}
                {chevron ? (
                    <Icon name="chevron-down" className="size-3 shrink-0" />
                ) : null}
            </Button>

            {open ? (
                <div
                    role="menu"
                    onClick={() => setOpen(false)}
                    className={`absolute top-full z-10 mt-3 min-w-42 rounded-[5px] border border-border-control bg-surface p-2 shadow-lg ${align === "end" ? "right-0" : "left-0"}`}
                >
                    {children}
                </div>
            ) : null}
        </div>
    );
}

export function MenuItem({
    to,
    children,
}: {
    to: string;
    children: ReactNode;
}) {
    return (
        <NavLink
            to={to}
            role="menuitem"
            className={({ isActive }) =>
                `block rounded-[3px] px-3 py-2 text-sm ${
                    isActive
                        ? "bg-surface-head font-bold text-ink"
                        : "text-ink hover:bg-surface-head"
                }`
            }
        >
            {children}
        </NavLink>
    );
}

export function MenuButton({
    onClick,
    active = false,
    children,
}: {
    onClick: () => void;
    active?: boolean;
    children: ReactNode;
}) {
    return (
        <button
            type="button"
            role="menuitem"
            onClick={onClick}
            className={`block w-full cursor-pointer rounded-[3px] px-3 py-2 text-left text-sm ${active ? "bg-surface-head font-bold text-ink" : "text-ink hover:bg-surface-head"}`}
        >
            {children}
        </button>
    );
}
