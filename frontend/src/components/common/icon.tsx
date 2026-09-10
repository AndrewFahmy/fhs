export interface IconProps {
    name: IconName;
    className?: string;
}

function Icon({ name, className = "size-4" }: IconProps) {
    const ICONS_VERSION = "1";

    return (
        <svg aria-hidden="true" className={className}>
            <use href={`/icons.svg?v=${ICONS_VERSION}#${name}`} />
        </svg>
    );
}

export default Icon;

export type IconName =
    | "logo"
    | "menu"
    | "chevron-down"
    | "chevron-left"
    | "sun"
    | "moon"
    | "monitor"
    | "plus"
    | "close"
    | "alert";
