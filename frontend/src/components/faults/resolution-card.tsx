import { formatWhen, fullTimestamp } from "@/utils/format";

export interface ResolutionCardProps {
    resolution: string | null;
    resolvedBy: string | null;
    resolvedAt: string | null;
    /** Shown while the fault is still open. */
    openNote: string;
}

function ResolutionCard({
    resolution,
    resolvedBy,
    resolvedAt,
    openNote,
}: ResolutionCardProps) {
    return (
        <div className="rounded-[5px] border border-border-subtle bg-surface-subtle px-6 py-5">
            <p className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                Resolution
            </p>
            {resolvedAt ? (
                <>
                    <p className="mt-4 font-display text-[19px] leading-relaxed text-ink">
                        {resolution}
                    </p>
                    <p className="mt-6 text-sm text-ink-muted">
                        Resolved by {resolvedBy}
                    </p>
                    <p
                        className="text-sm text-ink-muted"
                        title={fullTimestamp(resolvedAt)}
                    >
                        {formatWhen(resolvedAt)}
                    </p>
                </>
            ) : (
                <>
                    <p className="mt-4 font-display text-[19px] text-ink-muted">
                        Not resolved
                    </p>
                    <p className="mt-3 text-sm text-ink-muted">{openNote}</p>
                </>
            )}
        </div>
    );
}

export default ResolutionCard;
