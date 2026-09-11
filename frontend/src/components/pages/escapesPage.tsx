import { Link } from "react-router";
import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { EscapeListItem, PagedResponse } from "@/api/types";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import SeverityMark from "@/components/common/severity-mark";
import { Button } from "@/components/controls/button";
import { buttonClasses } from "@/utils/button-helpers";
import CustomerCombobox from "@/components/customers/customer-combobox";
import FaultFilters from "@/components/faults/fault-filters";
import DataGrid, { type DataGridColumn } from "@/components/grid/data-grid";
import Pagination from "@/components/grid/pagination";
import { escapePath, paths } from "@/routes/navigation";
import { formatTimestamp, fullTimestamp } from "@/utils/format";
import { useFaultFilters } from "@/utils/use-fault-filters";

function EscapesPage() {
    const filters = useFaultFilters("customerCode");

    const { data, error, loading, refetch } = useGet<
        PagedResponse<EscapeListItem>
    >(`${endpoints.getEscapes}?${filters.query}`);

    const columns: DataGridColumn<EscapeListItem>[] = [
        {
            key: "severity",
            header: "Severity",
            width: "140px",
            loaderClassName: "w-24",
            render: (escape) => (
                <SeverityMark
                    severity={escape.severity}
                    muted={escape.resolvedAt !== null}
                />
            ),
        },
        {
            key: "customer",
            header: "Customer",
            width: "140px",
            className: "font-mono font-bold",
            loaderClassName: "w-14",
            render: (escape) => escape.customerCode,
        },
        {
            key: "errorCode",
            header: "Error code",
            width: "140px",
            className: "font-mono font-bold",
            loaderClassName: "w-20",
            render: (escape) => escape.errorCode,
        },
        {
            key: "description",
            header: "Customer report",
            width: "minmax(0,1fr)",
            className: "truncate",
            loaderClassName: "w-full",
            title: (escape) => escape.description,
            render: (escape) => escape.description,
        },
        {
            key: "reportedBy",
            header: "Reported by",
            width: "170px",
            className: "truncate",
            loaderClassName: "w-24",
            render: (escape) => escape.reportedBy,
        },
        {
            key: "reported",
            header: "Reported",
            width: "110px",
            loaderClassName: "w-10",
            title: (escape) => fullTimestamp(escape.reportedAt),
            render: (escape) => formatTimestamp(escape.reportedAt),
        },
    ];

    if (filters.status !== "open") {
        columns.push({
            key: "resolved",
            header: "Resolved",
            width: "110px",
            loaderClassName: "w-10",
            title: (escape) =>
                escape.resolvedAt ? fullTimestamp(escape.resolvedAt) : "",
            render: (escape) =>
                escape.resolvedAt ? formatTimestamp(escape.resolvedAt) : "—",
        });
    }

    const empty = filters.filtered ? (
        <EmptyState
            title="No escapes match these filters."
            action={
                <Button variant="secondary" onClick={filters.clear}>
                    Clear filters
                </Button>
            }
        />
    ) : (
        <EmptyState
            title="No open escapes."
            description="No customer report is waiting on a response."
            action={
                <Link to={paths.newEscape} className={buttonClasses()}>
                    New escape
                </Link>
            }
        />
    );

    return (
        <>
            <header className="flex flex-wrap items-start justify-between gap-4">
                <div>
                    <h1 className="font-display text-4xl font-bold text-ink">
                        Escapes
                    </h1>
                    <p className="mt-2 text-sm text-ink-muted">
                        Customer-reported faults requiring a response
                    </p>
                </div>
                <Link to={paths.newEscape} className={buttonClasses()}>
                    <Icon name="plus" className="size-4" />
                    New escape
                </Link>
            </header>

            <FaultFilters filters={filters}>
                <CustomerCombobox
                    placeholder="All customers"
                    clearLabel="All customers"
                    value={filters.get("customerCode")}
                    onChange={(code) => filters.set("customerCode", code)}
                />
            </FaultFilters>

            <div className="mt-8">
                {error ? (
                    <ErrorState
                        error={error}
                        title="Escapes could not be loaded."
                        onRetry={refetch}
                    />
                ) : (
                    <DataGrid
                        columns={columns}
                        items={data?.items}
                        loading={loading}
                        rowKey={(escape) => escape.escapeId}
                        rowHref={(escape) => escapePath(escape.escapeId)}
                        empty={empty}
                        footer={
                            data && data.totalCount > 0 ? (
                                <Pagination
                                    page={filters.page}
                                    pageSize={data.pageSize}
                                    totalCount={data.totalCount}
                                    totalPages={data.totalPages}
                                    onPageChange={filters.setPage}
                                />
                            ) : null
                        }
                    />
                )}
            </div>
        </>
    );
}

export default EscapesPage;
