import type { ReactNode } from "react";

export interface DetailFieldProps {
    label: string;
    children: ReactNode;
}

function DetailField({ label, children }: DetailFieldProps) {
    return (
        <div className="border-b border-border-subtle px-6 py-5 last:border-b-0">
            <p className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                {label}
            </p>
            <p className="mt-2 text-[15px] text-ink">{children}</p>
        </div>
    );
}

export default DetailField;
