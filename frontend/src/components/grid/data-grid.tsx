import type { ReactNode } from "react";
import { Link } from "react-router";
import Loader from "../common/loader";

export interface DataGridColumn<TItem> {
    key: string;
    header: string;
    width: string;
    render: (item: TItem) => ReactNode;
    className?: string;
    title?: (item: TItem) => string;
    loaderClassName?: string;
}

export interface DataGridProps<TItem> {
    columns: DataGridColumn<TItem>[];
    items: TItem[] | undefined;
    loading: boolean;
    rowKey: (item: TItem) => string;
    rowHref?: (item: TItem) => string;
    empty?: ReactNode;
    footer?: ReactNode;
    minWidth?: string;
    loadingRows?: number;
}

const rowClass =
    "grid items-center gap-4 border-t border-border-subtle px-6 py-7 text-sm text-ink";

function DataGrid<TItem>({
    columns,
    items,
    loading,
    rowKey,
    rowHref,
    empty,
    footer,
    minWidth = "62rem",
    loadingRows = 8,
}: DataGridProps<TItem>) {
    const template = {
        gridTemplateColumns: columns.map((column) => column.width).join(" "),
    };

    const isEmpty = !loading && (items?.length ?? 0) === 0;

    return (
        <div className="rounded-[5px] border border-border-strong bg-surface">
            <div className="overflow-x-auto">
                <div style={{ minWidth }}>
                    <div
                        className="grid items-center gap-4 rounded-t-[5px] bg-surface-head px-6 py-4"
                        style={template}
                    >
                        {columns.map((column) => (
                            <span
                                key={column.key}
                                className="text-[11px] font-bold uppercase tracking-wide text-ink-head"
                            >
                                {column.header}
                            </span>
                        ))}
                    </div>

                    {loading
                        ? Array.from({ length: loadingRows }, (_, row) => (
                              <div
                                  key={row}
                                  className={rowClass}
                                  style={template}
                              >
                                  {columns.map((column) => (
                                      <Loader
                                          key={column.key}
                                          className={`h-4 ${column.loaderClassName ?? "w-20"}`}
                                      />
                                  ))}
                              </div>
                          ))
                        : items?.map((item) => {
                              const cells = columns.map((column) => (
                                  <span
                                      key={column.key}
                                      className={column.className}
                                      title={column.title?.(item)}
                                  >
                                      {column.render(item)}
                                  </span>
                              ));

                              const href = rowHref?.(item);

                              return href ? (
                                  <Link
                                      key={rowKey(item)}
                                      to={href}
                                      className={`${rowClass} transition-colors hover:bg-surface-subtle`}
                                      style={template}
                                  >
                                      {cells}
                                  </Link>
                              ) : (
                                  <div
                                      key={rowKey(item)}
                                      className={rowClass}
                                      style={template}
                                  >
                                      {cells}
                                  </div>
                              );
                          })}
                </div>
            </div>

            {isEmpty && empty ? (
                <div className="border-t border-border-subtle p-6">{empty}</div>
            ) : null}

            {footer}
        </div>
    );
}

export default DataGrid;
