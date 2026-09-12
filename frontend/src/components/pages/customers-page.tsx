import { useState } from "react";
import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { CustomerListItem } from "@/api/types";
import EmptyState from "@/components/common/empty-state";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import { Button } from "@/components/controls/button";
import DataGrid, { type DataGridColumn } from "@/components/grid/data-grid";
import ActiveFilter from "@/components/lookups/active-filter";
import CreateLookupDialog from "@/components/lookups/create-lookup-dialog";
import LifecycleDialog from "@/components/lookups/lifecycle-dialog";
import { useIsAdmin } from "@/utils/use-is-admin";

function CustomersPage() {
    const isAdmin = useIsAdmin();
    const [showAll, setShowAll] = useState(false);
    const [creating, setCreating] = useState(false);
    const [deactivating, setDeactivating] = useState<CustomerListItem | null>(
        null,
    );

    const { data, error, loading, refetch } = useGet<CustomerListItem[]>(
        `${endpoints.getCustomers}?includeInactive=true`,
    );

    const all = data ?? [];
    const customers = showAll
        ? all
        : all.filter((customer) => customer.isActive);

    const columns: DataGridColumn<CustomerListItem>[] = [
        {
            key: "code",
            header: "Code",
            width: "160px",
            className: "font-mono font-bold",
            loaderClassName: "w-16",
            card: "code",
            render: (customer) => customer.code,
        },
        {
            key: "name",
            header: "Name",
            width: "minmax(0,1fr)",
            className: "truncate",
            loaderClassName: "w-48",
            card: "primary",
            title: (customer) => customer.name,
            render: (customer) => customer.name,
        },
        {
            key: "status",
            header: "Status",
            width: "160px",
            loaderClassName: "w-16",
            card: "meta",
            render: (customer) =>
                customer.isActive ? (
                    "Active"
                ) : (
                    <span className="text-ink-muted">Deactivated</span>
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
            render: (customer) =>
                customer.isActive ? (
                    <Button
                        variant="link"
                        size="sm"
                        onClick={() => setDeactivating(customer)}
                    >
                        Deactivate
                    </Button>
                ) : null,
        });
    }

    function closeAndRefetch() {
        setCreating(false);
        setDeactivating(null);
        refetch();
    }

    const empty =
        all.length === 0 ? (
            <EmptyState
                title="No customers yet."
                description="Escapes are reported for a customer."
                action={
                    isAdmin ? (
                        <Button onClick={() => setCreating(true)}>
                            New customer
                        </Button>
                    ) : null
                }
            />
        ) : (
            <EmptyState
                title="No active customers."
                description="Every customer has been deactivated."
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
                        Customers
                    </h1>
                    <p className="mt-2 text-sm text-ink-muted">
                        The customers who report escapes
                    </p>
                </div>
                {isAdmin ? (
                    <Button onClick={() => setCreating(true)}>
                        <Icon name="plus" className="size-4" />
                        New customer
                    </Button>
                ) : null}
            </header>

            <ActiveFilter
                showAll={showAll}
                onChange={setShowAll}
                count={loading || error ? undefined : customers.length}
                noun="customer"
            />

            <div className="mt-8">
                {error ? (
                    <ErrorState
                        error={error}
                        title="Customers could not be loaded."
                        onRetry={refetch}
                    />
                ) : (
                    <DataGrid
                        columns={columns}
                        items={customers}
                        loading={loading}
                        rowKey={(customer) => customer.customerId}
                        empty={empty}
                    />
                )}
            </div>

            {creating ? (
                <CreateLookupDialog
                    kind="customer"
                    onClose={() => setCreating(false)}
                    onCreated={closeAndRefetch}
                />
            ) : null}

            {deactivating ? (
                <LifecycleDialog
                    kind="customer"
                    id={deactivating.customerId}
                    code={deactivating.code}
                    name={deactivating.name}
                    onClose={() => setDeactivating(null)}
                    onDone={closeAndRefetch}
                />
            ) : null}
        </>
    );
}

export default CustomersPage;
