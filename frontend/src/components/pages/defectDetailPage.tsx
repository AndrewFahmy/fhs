import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { DefectDetailResponse } from "@/api/types";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import Loader from "@/components/common/loader";
import { Button, buttonClasses } from "@/components/controls/button";
import SeverityMark from "@/components/defects/severity-mark";
import StatusPill from "@/components/defects/status-pill";
import ResolveDialog from "@/components/defects/resolve-defect-dialog";
import { paths } from "@/routes/navigation";
import {
    formatWhen,
    fullTimestamp,
    stringTemplateFormat,
} from "@/utils/format";
import { useState, type ReactNode } from "react";
import { Link, useParams } from "react-router";

const labelClass =
    "text-[11px] font-bold uppercase tracking-wide text-ink-muted";

const cardClass = "rounded-[5px] border border-border-subtle bg-surface-subtle";

function Field({ label, children }: { label: string; children: ReactNode }) {
    return (
        <div className="border-b border-border-subtle px-6 py-5 last:border-b-0">
            <p className={labelClass}>{label}</p>
            <p className="mt-2 text-[15px] text-ink">{children}</p>
        </div>
    );
}

function DefectDetailPage() {
    const { defectId = "" } = useParams();
    const [resolving, setResolving] = useState(false);

    const { data, error, loading, refetch } = useGet<DefectDetailResponse>(
        stringTemplateFormat(endpoints.getDefectDetails, defectId),
    );

    const backLink = (
        <Link
            to={paths.defects}
            className="inline-flex items-center gap-2 text-sm font-bold uppercase text-ink-head hover:text-ink"
        >
            <Icon name="chevron-left" className="size-3" />
            Defects
        </Link>
    );

    if (error?.status === 404) {
        return (
            <>
                {backLink}
                <div className="mt-8">
                    <EmptyState
                        title="This defect no longer exists."
                        description="It may have been removed since the list was loaded."
                        action={
                            <Link
                                to={paths.defects}
                                className={buttonClasses({
                                    variant: "secondary",
                                })}
                            >
                                Back to defects
                            </Link>
                        }
                    />
                </div>
            </>
        );
    }

    if (error) {
        return (
            <>
                {backLink}
                <div className="mt-8">
                    <ErrorState
                        error={error}
                        title="This defect could not be loaded."
                        onRetry={refetch}
                    />
                </div>
            </>
        );
    }

    if (loading || !data) {
        return (
            <>
                {backLink}
                <Loader className="mt-6 h-10 w-80" />
                <Loader className="mt-4 h-4 w-64" />
                <div className="mt-10 grid gap-6 lg:grid-cols-[2fr_1fr]">
                    <Loader className="h-80 w-full" />
                    <Loader className="h-80 w-full" />
                </div>
            </>
        );
    }

    const resolved = data.resolvedAt !== null;

    return (
        <>
            {backLink}

            <header className="mt-6 flex flex-wrap items-center justify-between gap-4">
                <div>
                    <h1 className="font-display text-[38px] leading-tight font-bold text-ink">
                        {data.stationCode} / {data.errorCode}
                    </h1>
                    <p className="mt-1 text-sm text-ink-muted">
                        <span title={fullTimestamp(data.createdAt)}>
                            Raised {formatWhen(data.createdAt)}
                        </span>{" "}
                        by {data.raisedBy}
                    </p>
                </div>
                <div className="flex items-center gap-3">
                    <SeverityMark severity={data.severity} badge />
                    <StatusPill resolved={resolved} />
                    {resolved ? null : (
                        <Button onClick={() => setResolving(true)}>
                            Resolve defect
                        </Button>
                    )}
                </div>
            </header>

            <div className="mt-8 border-t border-border-strong pt-8">
                <p className={labelClass}>Description</p>
                <p className="mt-4 max-w-4xl font-display text-[23px] leading-relaxed text-ink">
                    {data.description}
                </p>
            </div>

            <div className="mt-10 grid gap-6 lg:grid-cols-[2fr_1fr]">
                <div className={cardClass}>
                    <Field label="Station">
                        <span className="font-mono font-bold">
                            {data.stationCode}
                        </span>{" "}
                        <span className="text-ink-muted">
                            {data.stationName}
                        </span>
                    </Field>
                    <Field label="Error code">
                        <span className="font-mono font-bold">
                            {data.errorCode}
                        </span>{" "}
                        <span className="text-ink-muted">
                            {data.errorCodeDescription}
                        </span>
                    </Field>
                    <Field label="Raised by">{data.raisedBy}</Field>
                    <Field label="Raised at">
                        <span title={fullTimestamp(data.createdAt)}>
                            {formatWhen(data.createdAt)}
                        </span>
                    </Field>
                </div>

                <div className={`${cardClass} px-6 py-5`}>
                    <p className={labelClass}>Resolution</p>
                    {resolved ? (
                        <>
                            <p className="mt-4 font-display text-[19px] leading-relaxed text-ink">
                                {data.resolution}
                            </p>
                            <p className="mt-6 text-sm text-ink-muted">
                                Resolved by {data.resolvedBy}
                            </p>
                            <p
                                className="text-sm text-ink-muted"
                                title={
                                    data.resolvedAt
                                        ? fullTimestamp(data.resolvedAt)
                                        : undefined
                                }
                            >
                                {data.resolvedAt
                                    ? formatWhen(data.resolvedAt)
                                    : ""}
                            </p>
                        </>
                    ) : (
                        <>
                            <p className="mt-4 font-display text-[19px] text-ink-muted">
                                Not resolved
                            </p>
                            <p className="mt-3 text-sm text-ink-muted">
                                An open defect remains visible to the next
                                operator.
                            </p>
                        </>
                    )}
                </div>
            </div>

            {resolving ? (
                <ResolveDialog
                    defect={data}
                    onClose={() => setResolving(false)}
                    onResolved={() => {
                        setResolving(false);
                        refetch();
                    }}
                />
            ) : null}
        </>
    );
}

export default DefectDetailPage;
