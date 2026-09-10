import type { ReactNode } from "react";

export interface EmptyStateProps {
    title: string;
    description?: string;
    action?: ReactNode;
}

function EmptyState({ title, description, action }: EmptyStateProps) {
    return (
        <div className="flex flex-col items-center justify-center gap-3 rounded-lg border-border-subtle bg-surface-subtle px-6 py-16 text-center">
            <p className="font-display text-lg font-bold text-ink-muted">
                {title}
            </p>
            {description && (
                <p className="max-w-md text-sm text-ink-muted">{description}</p>
            )}
            {action ? <div className="mt-2">{action}</div> : null}
        </div>
    );
}

export default EmptyState;
