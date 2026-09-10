import type { Severity } from "@/api/enums";

const swatchClasses: Record<Severity, string> = {
    Critical: "bg-severity-critical",
    Major: "bg-severity-major",
    Minor: "border-[1.5px] border-severity-minor bg-surface",
};

export interface SeverityMarkProps {
    severity?: Severity;
    muted?: boolean;
}

function SeverityMark({ severity, muted = false }: SeverityMarkProps) {
    return (
        <span className="inline-flex items-center gap-2.5">
            <span
                className={`size-3 shrink-0 rounded-[1px] ${severity && swatchClasses[severity]} ${muted ? "opacity-45" : ""}`}
            />
            <span className="text-xs font-bold uppercase text-ink">
                {severity}
            </span>
        </span>
    );
}

export default SeverityMark;
