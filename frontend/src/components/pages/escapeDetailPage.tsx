import { useState } from "react";
import { Link, useParams } from "react-router";
import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { EscapeDetailResponse } from "@/api/types";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Loader from "@/components/common/loader";
import SeverityMark from "@/components/common/severity-mark";
import StatusPill from "@/components/common/status-pill";
import { Button } from "@/components/controls/button";
import { buttonClasses } from "@/utils/button-helpers";
import DetailField from "@/components/faults/detail-field";
import ResolutionCard from "@/components/faults/resolution-card";
import ResolveDialog from "@/components/faults/resolve-dialog";
import BackLink from "@/components/navigation/back-link";
import { paths } from "@/routes/navigation";
import {
    formatWhen,
    fullTimestamp,
    stringTemplateFormat,
} from "@/utils/format";

function EscapeDetailPage() {
    const { escapeId = "" } = useParams();
    const [resolving, setResolving] = useState(false);

    const { data, error, loading, refetch } = useGet<EscapeDetailResponse>(
        stringTemplateFormat(endpoints.getEscapeDetails, escapeId),
    );

    const backLink = <BackLink to={paths.escapes}>Escapes</BackLink>;

    if (error?.status === 404) {
        return (
            <>
                {backLink}
                <div className="mt-8">
                    <EmptyState
                        title="This escape no longer exists."
                        description="It may have been removed since the list was loaded."
                        action={
                            <Link
                                to={paths.escapes}
                                className={buttonClasses({
                                    variant: "secondary",
                                })}
                            >
                                Back to escapes
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
                        title="This escape could not be loaded."
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
                        {data.customerCode} / {data.errorCode}
                    </h1>
                    <p className="mt-1 text-sm text-ink-muted">
                        <span title={fullTimestamp(data.reportedAt)}>
                            Reported {formatWhen(data.reportedAt)}
                        </span>{" "}
                        by {data.reportedBy}
                    </p>
                </div>
                <div className="flex items-center gap-3">
                    <SeverityMark severity={data.severity} badge />
                    <StatusPill resolved={resolved} />
                    {resolved ? null : (
                        <Button onClick={() => setResolving(true)}>
                            Resolve escape
                        </Button>
                    )}
                </div>
            </header>

            <div className="mt-8 border-t border-border-strong pt-8">
                <p className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                    Customer report
                </p>
                <p className="mt-4 max-w-4xl font-display text-[23px] leading-relaxed text-ink">
                    {data.description}
                </p>
            </div>

            <div className="mt-10 grid gap-6 lg:grid-cols-[2fr_1fr]">
                <div className="rounded-[5px] border border-border-subtle bg-surface-subtle">
                    <DetailField label="Customer">
                        <span className="font-mono font-bold">
                            {data.customerCode}
                        </span>{" "}
                        <span className="text-ink-muted">
                            {data.customerName}
                        </span>
                    </DetailField>
                    <DetailField label="Error code">
                        <span className="font-mono font-bold">
                            {data.errorCode}
                        </span>{" "}
                        <span className="text-ink-muted">
                            {data.errorCodeDescription}
                        </span>
                    </DetailField>
                    <DetailField label="Reported by">
                        {data.reportedBy}
                    </DetailField>
                    <DetailField label="Reported at">
                        <span title={fullTimestamp(data.reportedAt)}>
                            {formatWhen(data.reportedAt)}
                        </span>
                    </DetailField>
                </div>

                <ResolutionCard
                    resolution={data.resolution}
                    resolvedBy={data.resolvedBy}
                    resolvedAt={data.resolvedAt}
                    openNote="An open escape remains visible until the customer has a response."
                />
            </div>

            {resolving ? (
                <ResolveDialog
                    kind="escape"
                    id={data.escapeId}
                    severity={data.severity}
                    sourceCode={data.customerCode}
                    errorCode={data.errorCode}
                    openedAt={data.reportedAt}
                    openedBy={data.reportedBy}
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

export default EscapeDetailPage;
