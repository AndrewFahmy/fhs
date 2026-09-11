import { buttonClasses, type ButtonProps } from "@/utils/button-helpers";

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
