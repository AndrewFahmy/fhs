import { useState } from "react";
import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { StationListItem } from "@/api/types";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import { Button } from "@/components/controls/button";
import DataGrid, { type DataGridColumn } from "@/components/grid/data-grid";
import ActiveFilter from "@/components/lookups/active-filter";
import CreateLookupDialog from "@/components/lookups/create-lookup-dialog";
import LifecycleDialog from "@/components/lookups/lifecycle-dialog";
import { useIsAdmin } from "@/utils/use-is-admin";

function StationsPage() {
    const isAdmin = useIsAdmin();
    const [showAll, setShowAll] = useState(false);
    const [creating, setCreating] = useState(false);
    const [decommissioning, setDecommissioning] =
        useState<StationListItem | null>(null);

    const { data, error, loading, refetch } = useGet<StationListItem[]>(
        `${endpoints.getStations}?includeInactive=true`,
    );

    const all = data ?? [];
    const stations = showAll ? all : all.filter((station) => station.isActive);

    const columns: DataGridColumn<StationListItem>[] = [
        {
            key: "code",
            header: "Code",
            width: "160px",
            className: "font-mono font-bold",
            loaderClassName: "w-16",
            card: "code",
            render: (station) => station.code,
        },
        {
            key: "name",
            header: "Name",
            width: "minmax(0,1fr)",
            className: "truncate",
            loaderClassName: "w-48",
            card: "primary",
            title: (station) => station.name,
            render: (station) => station.name,
        },
        {
            key: "status",
            header: "Status",
            width: "160px",
            loaderClassName: "w-16",
            card: "meta",
            render: (station) =>
                station.isActive ? (
                    "Active"
                ) : (
                    <span className="text-ink-muted">Decommissioned</span>
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
            card: "actions",
            render: (station) =>
                station.isActive ? (
                    <Button
                        variant="link"
                        size="sm"
                        onClick={() => setDecommissioning(station)}
                    >
                        Decommission
                    </Button>
                ) : null,
        });
    }

    function closeAndRefetch() {
        setCreating(false);
        setDecommissioning(null);
        refetch();
    }

    const empty =
        all.length === 0 ? (
            <EmptyState
                title="No stations yet."
                description="Defects are raised against a station."
                action={
                    isAdmin ? (
                        <Button onClick={() => setCreating(true)}>
                            New station
                        </Button>
                    ) : null
                }
            />
        ) : (
            <EmptyState
                title="No active stations."
                description="Every station has been decommissioned."
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
                        Stations
                    </h1>
                    <p className="mt-2 text-sm text-ink-muted">
                        The points on the line where defects are raised
                    </p>
                </div>
                {isAdmin ? (
                    <Button onClick={() => setCreating(true)}>
                        <Icon name="plus" className="size-4" />
                        New station
                    </Button>
                ) : null}
            </header>

            <ActiveFilter
                showAll={showAll}
                onChange={setShowAll}
                count={loading || error ? undefined : stations.length}
                noun="station"
            />

            <div className="mt-8">
                {error ? (
                    <ErrorState
                        error={error}
                        title="Stations could not be loaded."
                        onRetry={refetch}
                    />
                ) : (
                    <DataGrid
                        columns={columns}
                        items={stations}
                        loading={loading}
                        rowKey={(station) => station.stationId}
                        empty={empty}
                    />
                )}
            </div>

            {creating ? (
                <CreateLookupDialog
                    kind="station"
                    onClose={() => setCreating(false)}
                    onCreated={closeAndRefetch}
                />
            ) : null}

            {decommissioning ? (
                <LifecycleDialog
                    kind="station"
                    id={decommissioning.stationId}
                    code={decommissioning.code}
                    name={decommissioning.name}
                    onClose={() => setDecommissioning(null)}
                    onDone={closeAndRefetch}
                />
            ) : null}
        </>
    );
}

export default StationsPage;
