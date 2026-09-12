import type { ReactNode } from "react";
import { Link } from "react-router";
import Loader from "@/components/common/loader";
import { mdQuery, useMediaQuery } from "@/utils/use-media-query";

export type DataGridCardRole =
    | "badge"
    | "code"
    | "primary"
    | "meta"
    | "actions";

export interface DataGridColumn<TItem> {
    key: string;
    header: string;
    width: string;
    render: (item: TItem) => ReactNode;
    className?: string;
    title?: (item: TItem) => string;
    loaderClassName?: string;
    /** Where the column lands on the card layout below `md`. */
    card?: DataGridCardRole;
    /** Prefix for a `meta` value that would read as ambiguous on a card. */
    cardLabel?: string;
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

const cardClass = "rounded-[5px] border border-border-strong bg-surface";

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
    const compact = !useMediaQuery(mdQuery);

    const template = {
        gridTemplateColumns: columns.map((column) => column.width).join(" "),
    };

    const isEmpty = !loading && (items?.length ?? 0) === 0;

    if (compact) {
        const inRole = (role: DataGridCardRole) =>
            columns.filter((column) => column.card === role);

        const badges = inRole("badge");
        const codes = inRole("code");
        const primary = inRole("primary");
        const meta = inRole("meta");
        const actions = inRole("actions");

        const composed =
            badges.length + codes.length + primary.length + meta.length > 0;

        return (
            <div className="flex flex-col gap-3">
                {loading
                    ? Array.from(
                          { length: Math.min(loadingRows, 4) },
                          (_, row) => (
                              <div
                                  key={row}
                                  className={`${cardClass} flex flex-col gap-3 px-5 py-4`}
                              >
                                  <Loader className="h-3 w-20" />
                                  <Loader className="h-4 w-32" />
                                  <Loader className="h-4 w-full" />
                                  <Loader className="h-3 w-24" />
                              </div>
                          ),
                      )
                    : items?.map((item) => {
                          const body = composed ? (
                              <div className="flex flex-col gap-2 px-5 py-4">
                                  {badges.length > 0 ? (
                                      <div className="flex flex-wrap items-center gap-3">
                                          {badges.map((column) => (
                                              <span key={column.key}>
                                                  {column.render(item)}
                                              </span>
                                          ))}
                                      </div>
                                  ) : null}

                                  {codes.length > 0 ? (
                                      <div className="flex flex-wrap items-center gap-2 font-mono text-[13px] font-bold text-ink">
                                          {codes.map((column, index) => (
                                              <span
                                                  key={column.key}
                                                  className="flex items-center gap-2"
                                              >
                                                  {index > 0 ? (
                                                      <span className="text-ink-muted">
                                                          /
                                                      </span>
                                                  ) : null}
                                                  {column.render(item)}
                                              </span>
                                          ))}
                                      </div>
                                  ) : null}

                                  {primary.map((column) => (
                                      <p
                                          key={column.key}
                                          className="line-clamp-2 text-sm text-ink"
                                      >
                                          {column.render(item)}
                                      </p>
                                  ))}

                                  {meta.length > 0 ? (
                                      <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-[11px] text-ink-muted">
                                          {meta.map((column) => (
                                              <span key={column.key}>
                                                  {column.cardLabel
                                                      ? `${column.cardLabel} `
                                                      : null}
                                                  {column.render(item)}
                                              </span>
                                          ))}
                                      </div>
                                  ) : null}
                              </div>
                          ) : (
                              <dl className="flex flex-col gap-2 px-5 py-4">
                                  {columns.map((column) => (
                                      <div
                                          key={column.key}
                                          className="flex items-baseline justify-between gap-4"
                                      >
                                          <dt className="text-[11px] font-bold tracking-wide text-ink-head uppercase">
                                              {column.header}
                                          </dt>
                                          <dd className="text-right text-sm text-ink">
                                              {column.render(item)}
                                          </dd>
                                      </div>
                                  ))}
                              </dl>
                          );

                          const actionCells = actions
                              .map((column) => ({
                                  key: column.key,
                                  node: column.render(item),
                              }))
                              .filter((cell) => Boolean(cell.node));

                          const href = rowHref?.(item);

                          return (
                              <div key={rowKey(item)} className={cardClass}>
                                  {href ? (
                                      <Link
                                          to={href}
                                          className="block rounded-[5px] transition-colors hover:bg-surface-subtle"
                                      >
                                          {body}
                                      </Link>
                                  ) : (
                                      body
                                  )}

                                  {actionCells.length > 0 ? (
                                      <div className="flex flex-wrap items-center gap-4 border-t border-border-subtle px-5 py-3">
                                          {actionCells.map((cell) => (
                                              <span key={cell.key}>
                                                  {cell.node}
                                              </span>
                                          ))}
                                      </div>
                                  ) : null}
                              </div>
                          );
                      })}

                {isEmpty && empty ? (
                    <div className={`${cardClass} p-6`}>{empty}</div>
                ) : null}

                {footer}
            </div>
        );
    }

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
                                className="text-[11px] font-bold tracking-wide text-ink-head uppercase"
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
