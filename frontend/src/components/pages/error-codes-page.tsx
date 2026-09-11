import { useState } from "react";
import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { ErrorCodeListItem } from "@/api/types";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import SeverityMark from "@/components/common/severity-mark";
import { Button } from "@/components/controls/button";
import DataGrid, { type DataGridColumn } from "@/components/grid/data-grid";
import ActiveFilter from "@/components/lookups/active-filter";
import CreateErrorCodeDialog from "@/components/lookups/create-error-code-dialog";
import LifecycleDialog from "@/components/lookups/lifecycle-dialog";
import { useIsAdmin } from "@/utils/use-is-admin";

function ErrorCodesPage() {
    const isAdmin = useIsAdmin();
    const [showAll, setShowAll] = useState(false);
    const [creating, setCreating] = useState(false);
    const [retiring, setRetiring] = useState<ErrorCodeListItem | null>(null);

    const { data, error, loading, refetch } = useGet<ErrorCodeListItem[]>(
        `${endpoints.getErrorCodes}?includeInactive=true`,
    );

    const all = data ?? [];
    const errorCodes = showAll
        ? all
        : all.filter((errorCode) => errorCode.isActive);

    const columns: DataGridColumn<ErrorCodeListItem>[] = [
        {
            key: "severity",
            header: "Severity",
            width: "140px",
            loaderClassName: "w-24",
            render: (errorCode) => (
                <SeverityMark
                    severity={errorCode.severity}
                    muted={!errorCode.isActive}
                />
            ),
        },
        {
            key: "code",
            header: "Code",
            width: "160px",
            className: "font-mono font-bold",
            loaderClassName: "w-20",
            render: (errorCode) => errorCode.code,
        },
        {
            key: "description",
            header: "Description",
            width: "minmax(0,1fr)",
            className: "truncate",
            loaderClassName: "w-48",
            title: (errorCode) => errorCode.description,
            render: (errorCode) => errorCode.description,
        },
        {
            key: "status",
            header: "Status",
            width: "160px",
            loaderClassName: "w-16",
            render: (errorCode) =>
                errorCode.isActive ? (
                    "Active"
                ) : (
                    <span className="text-ink-muted">Retired</span>
                ),
        },
    ];

    if (isAdmin) {
        columns.push({
            key: "actions",
            header: "",
            width: "150px",
            className: "justify-self-end",
            loaderClassName: "w-24 justify-self-end",
            render: (errorCode) =>
                errorCode.isActive ? (
                    <Button
                        variant="link"
                        size="sm"
                        onClick={() => setRetiring(errorCode)}
                    >
                        Retire
                    </Button>
                ) : null,
        });
    }

    function closeAndRefetch() {
        setCreating(false);
        setRetiring(null);
        refetch();
    }

    const empty =
        all.length === 0 ? (
            <EmptyState
                title="No error codes yet."
                description="Defects and escapes are classified against an error code."
                action={
                    isAdmin ? (
                        <Button onClick={() => setCreating(true)}>
                            New error code
                        </Button>
                    ) : null
                }
            />
        ) : (
            <EmptyState
                title="No active error codes."
                description="Every error code has been retired."
                action={
                    <Button
                        variant="secondary"
                        onClick={() => setShowAll(true)}
                    >
                        Show all
                    </Button>
                }
            />
        );

    return (
        <>
            <header className="flex flex-wrap items-start justify-between gap-4">
                <div>
                    <h1 className="font-display text-4xl font-bold text-ink">
                        Error codes
                    </h1>
                    <p className="mt-2 text-sm text-ink-muted">
                        The fault taxonomy used to classify defects and escapes
                    </p>
                </div>
                {isAdmin ? (
                    <Button onClick={() => setCreating(true)}>
                        <Icon name="plus" className="size-4" />
                        New error code
                    </Button>
                ) : null}
            </header>

            <ActiveFilter
                showAll={showAll}
                onChange={setShowAll}
                count={loading || error ? undefined : errorCodes.length}
                noun="code"
            />

            <div className="mt-8">
                {error ? (
                    <ErrorState
                        error={error}
                        title="Error codes could not be loaded."
                        onRetry={refetch}
                    />
                ) : (
                    <DataGrid
                        columns={columns}
                        items={errorCodes}
                        loading={loading}
                        rowKey={(errorCode) => errorCode.errorCodeId}
                        empty={empty}
                    />
                )}
            </div>

            {creating ? (
                <CreateErrorCodeDialog
                    onClose={() => setCreating(false)}
                    onCreated={closeAndRefetch}
                />
            ) : null}

            {retiring ? (
                <LifecycleDialog
                    kind="errorCode"
                    id={retiring.errorCodeId}
                    code={retiring.code}
                    name={retiring.description}
                    onClose={() => setRetiring(null)}
                    onDone={closeAndRefetch}
                />
            ) : null}
        </>
    );
}

export default ErrorCodesPage;
