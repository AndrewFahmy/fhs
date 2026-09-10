import type { Severity } from "@/api/enums";

const swatchClasses: Record<Severity, string> = {
    Critical: "bg-severity-critical",
    Major: "bg-severity-major",
    Minor: "border-[1.5px] border-severity-minor bg-surface",
};

const badgeClasses: Record<Severity, string> = {
    Critical: "bg-severity-critical-surface text-severity-critical-ink",
    Major: "bg-surface-subtle text-ink",
    Minor: "bg-surface-subtle text-ink",
};

export interface SeverityMarkProps {
    severity?: Severity;
    muted?: boolean;
    badge?: boolean;
}

function SeverityMark({
    severity,
    muted = false,
    badge = false,
}: SeverityMarkProps) {
    if (!severity) {
        return null;
    }

    const content = (
        <>
            <span
                className={`size-3 shrink-0 rounded-[1px] ${swatchClasses[severity]} ${muted ? "opacity-45" : ""}`}
            />
            <span className="text-xs font-bold uppercase">{severity}</span>
        </>
    );

    if (!badge) {
        return (
            <span className="inline-flex item-center gap-2.5">{content}</span>
        );
    }

    return (
        <span
            className={`inline-flex h-8.5 items-center gap-2.5 rounded-full px-4 ${badgeClasses[severity]}`}
        >
            {content}
        </span>
    );
}

export default SeverityMark;
