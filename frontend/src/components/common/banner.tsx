import type { ReactNode } from "react";
import Icon from "@/components/common/icon";

export interface BannerProps {
    message: string;
    action?: ReactNode;
}

/** Inline, on the surface that caused it — for 409s and other reload-me states. */
function Banner({ message, action }: BannerProps) {
    return (
        <div className="rounded-sm border border-border-control bg-surface-subtle px-4 py-3">
            <p className="flex items-start gap-2.5 text-sm text-ink">
                <Icon name="alert" className="mt-0.5 size-4 shrink-0" />
                {message}
            </p>
            {action ? <div className="mt-3 pl-6.5">{action}</div> : null}
        </div>
    );
}

export default Banner;
