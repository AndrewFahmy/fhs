import type { ReactNode } from "react";

export interface NoticeProps {
    title?: string;
    children?: ReactNode;
    action?: ReactNode;
}

export function Notice({ title, children, action }: NoticeProps) {
    return (
        <div className="flex min-h-screen items-center justify-center bg-canvas p-6">
            <div className="w-full max-w-md rounded-lg border border-border-strong bg-surface p-8 text-center">
                <h1 className="font-display text-2xl font-bold text-ink">
                    {title}
                </h1>
                {children ? (
                    <p className="mt-3 text-sm text-ink-muted">{children}</p>
                ) : null}
                {action ? (
                    <div className="mt-6 flex justify-center">{action}</div>
                ) : null}
            </div>
        </div>
    );
}
