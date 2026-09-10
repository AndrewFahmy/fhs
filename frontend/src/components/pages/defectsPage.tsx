import { useGet } from "@/api/api-hooks";
import { endpoints } from "@/api/api-constants";
import { pageOf } from "@/api/helpers/common";
import {
    defectsPageHasFilters,
    parseDefectStatusFilter,
    mapQuery,
} from "@/api/helpers/defects";
import type { DefectListItem, PagedResponse } from "@/api/types";
import { Button, buttonClasses } from "@/components/common/button";
import DataGrid, { type DataGridColumn } from "@/components/grid/data-grid";
import Pagination from "@/components/grid/pagination";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import SeverityMark from "@/components/defects/severity-mark";
import { defectPath, paths } from "@/routes/navigation";
import { formatTimestamp, fullTimestamp } from "@/utils/format";
import { Link, useSearchParams } from "react-router";

function DefectsPage() {
    const [search, setSearch] = useSearchParams();

    const { data, error, loading, refetch } = useGet<
        PagedResponse<DefectListItem>
    >(`${endpoints.getDefects}?${mapQuery(search)}`);

    const showResolved = parseDefectStatusFilter(search) !== "open";
    const page = pageOf(search);

    const columns: DataGridColumn<DefectListItem>[] = [
        {
            key: "severity",
            header: "Severity",
            width: "140px",
            loaderClassName: "w-24",
            render: (defect) => (
                <SeverityMark
                    severity={defect.severity}
                    muted={defect.resolvedAt !== null}
                />
            ),
        },
        {
            key: "station",
            header: "Station",
            width: "110px",
            className: "font-mono font-bold",
            loaderClassName: "w-14",
            render: (defect) => defect.stationCode,
        },
        {
            key: "errorCode",
            header: "Error code",
            width: "140px",
            className: "font-mono font-bold",
            loaderClassName: "w-20",
            render: (defect) => defect.errorCode,
        },
        {
            key: "description",
            header: "Description",
            width: "minmax(0,1fr)",
            className: "truncate",
            loaderClassName: "w-full",
            title: (defect) => defect.description,
            render: (defect) => defect.description,
        },
        {
            key: "raisedBy",
            header: "Raised by",
            width: "170px",
            className: "truncate",
            loaderClassName: "w-24",
            render: (defect) => defect.raisedBy,
        },
        {
            key: "raised",
            header: "Raised",
            width: "110px",
            loaderClassName: "w-10",
            title: (defect) => fullTimestamp(defect.createdAt),
            render: (defect) => formatTimestamp(defect.createdAt),
        },
    ];

    if (showResolved) {
        columns.push({
            key: "resolved",
            header: "Resolved",
            width: "110px",
            loaderClassName: "w-10",
            title: (defect) =>
                defect.resolvedAt ? fullTimestamp(defect.resolvedAt) : "",
            render: (defect) =>
                defect.resolvedAt ? formatTimestamp(defect.resolvedAt) : "—",
        });
    }

    function goToPage(next: number) {
        setSearch((previous) => {
            const params = new URLSearchParams(previous);
            params.set("page", String(next));

            return params;
        });
    }

    const empty = defectsPageHasFilters(search) ? (
        <EmptyState
            title="No defects match these filters."
            action={
                <Button
                    variant="secondary"
                    onClick={() => setSearch(new URLSearchParams())}
                >
                    Clear filters
                </Button>
            }
        />
    ) : (
        <EmptyState
            title="No open defects."
            description="Nothing is waiting on the line right now."
            action={
                <Link to={paths.newDefect} className={buttonClasses()}>
                    New defect
                </Link>
            }
        />
    );

    return (
        <>
            <header className="flex flex-wrap items-start justify-between gap-4">
                <div>
                    <h1 className="font-display text-4xl font-bold text-ink">
                        Defects
                    </h1>
                    <p className="mt-2 text-sm text-ink-muted">
                        Open quality faults from the production line
                    </p>
                </div>
                <Link to={paths.newDefect} className={buttonClasses()}>
                    <Icon name="plus" className="size-4" />
                    New defect
                </Link>
            </header>

            <div className="mt-8">
                {error ? (
                    <ErrorState
                        error={error}
                        title="Defects could not be loaded."
                        onRetry={refetch}
                    />
                ) : (
                    <DataGrid
                        columns={columns}
                        items={data?.items}
                        loading={loading}
                        rowKey={(defect) => defect.defectId}
                        rowHref={(defect) => defectPath(defect.defectId)}
                        empty={empty}
                        footer={
                            data && data.totalCount > 0 ? (
                                <Pagination
                                    page={page}
                                    pageSize={data.pageSize}
                                    totalCount={data.totalCount}
                                    totalPages={data.totalPages}
                                    onPageChange={goToPage}
                                />
                            ) : null
                        }
                    />
                )}
            </div>
        </>
    );
}

export default DefectsPage;
