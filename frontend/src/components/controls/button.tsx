import type { ButtonHTMLAttributes } from "react";

export type ButtonVariant = "primary" | "secondary" | "subtle" | "ghost";
export type ButtonSize = "md" | "sm" | "icon" | "none";

const variantClasses: Record<ButtonVariant, string> = {
    primary: "bg-action text-action-ink hover:opacity-90",
    secondary:
        "border border-border-control bg-surface text-ink hover:bg-surface-subtle",
    subtle: "border border-border-control bg-surface-subtle text-ink hover:bg-surface-head",
    ghost: "text-ink",
};

const sizeClasses: Record<ButtonSize, string> = {
    md: "h-11 px-5",
    sm: "h-9 px-4",
    icon: "size-9",
    none: "",
};

export interface ButtonStyleOptions {
    variant?: ButtonVariant;
    size?: ButtonSize;
    className?: string;
}

export interface ButtonProps
    extends ButtonHTMLAttributes<HTMLButtonElement>, ButtonStyleOptions {}

export function buttonClasses({
    variant = "primary",
    size = "md",
    className = "",
}: ButtonStyleOptions = {}): string {
    return `inline-flex cursor-pointer items-center justify-center gap-2 rounded-sm text-sm whitespace-nowrap transition-opacity focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-border-focus disabled:pointer-events-none disabled:opacity-60 ${variantClasses[variant]} ${sizeClasses[size]} ${className}`;
}

export function Button({
    variant = "primary",
    size = "md",
    className = "",
    type = "button",
    ...props
}: ButtonProps) {
    return (
        <button
            type={type}
            className={buttonClasses({ variant, size, className })}
            {...props}
        ></button>
    );
}
