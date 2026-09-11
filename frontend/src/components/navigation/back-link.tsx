import type { ReactNode } from "react";
import { Link } from "react-router";
import Icon from "@/components/common/icon";

export interface BackLinkProps {
    to: string;
    children: ReactNode;
}

function BackLink({ to, children }: BackLinkProps) {
    return (
        <Link
            to={to}
            className="inline-flex items-center gap-2 text-sm font-bold uppercase text-ink-head hover:text-ink"
        >
            <Icon name="chevron-left" className="size-3" />
            {children}
        </Link>
    );
}

export default BackLink;
