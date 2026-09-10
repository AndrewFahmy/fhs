import type { ApiError } from "@/api/types";
import { Button } from "@/components/common/button";

export interface ErrorStateProps {
    error: ApiError;
    title?: string;
    onRetry?: () => void;
}

function ErrorState({ error, title, onRetry }: ErrorStateProps) {
    const forbidden = error.status === 403;

    const heading = forbidden
        ? "You don't have access to this"
        : (title ?? "Something went wrong.");

    const detail =
        error.problem?.detail ??
        (forbidden
            ? "Your account is signed in but is not a registered actor in FHS. As an administrator to add you."
            : error.message);

    return (
        <div className="flex flex-col items-center justify-center gap-3 rounded-lg border border-border-subtle bg-surface-subtle px-6 py-16 text-center">
            <p className="font-display text-lg font-bold text-ink">{heading}</p>
            <p className="max-w-md text-sm text-ink-muted">{detail}</p>
            {error.code ? (
                <p className="font-mono text-xs text-ink-muted">{error.code}</p>
            ) : null}
            {onRetry && !forbidden ? (
                <Button variant="secondary" className="mt-2" onClick={onRetry}>
                    Retry
                </Button>
            ) : null}
        </div>
    );
}

export default ErrorState;
