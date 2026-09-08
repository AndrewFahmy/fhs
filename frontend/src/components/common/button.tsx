import type { ButtonHTMLAttributes } from "react";

type ButtonVariant = "primary" | "secondary";

const variantClasses: Record<ButtonVariant, string> = {
    primary: "bg-action text-action-ink hover:opacity-90",
    secondary:
        "border border-border-control bg-surface text-ink hover:bg-surface-subtle",
};

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: ButtonVariant;
}

export function Button({
    variant = "primary",
    className = "",
    type = "button",
    ...props
}: ButtonProps) {
    return (
        <button
            type={type}
            className={`inline-flex h-11 items-center justify-center gap-2 rounded-sm px-5 text-sm font-bold whitespace-nowrap transition-opacity focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-border-focus disabled:pointer-events-none disabled:opacity-60 ${variantClasses[variant]} ${className}`}
            {...props}
        ></button>
    );
}
