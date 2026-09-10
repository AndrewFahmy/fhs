function StatusPill({ resolved }: { resolved: boolean }) {
    if (resolved) {
        return (
            <span className="inline-flex h-8.5 items-center gap-2 rounded-full border border-border-control px-4 text-xs font-bold uppercase text-ink-muted">
                <span className="size-2.5 rounded-full border-[1.5px] border-ink-muted" />
                Resolved
            </span>
        );
    }

    return (
        <span className="inline-flex h-8.5 items-center gap-2 rounded-full bg-action px-4 text-xs font-bold uppercase text-action-ink">
            <span className="size-2.5 rounded-full bg-action-ink" />
            Open
        </span>
    );
}

export default StatusPill;
