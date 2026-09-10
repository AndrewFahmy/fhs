import { Button } from "@/components/controls/button";

export interface PaginationProps {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

function Pagination({
    page,
    pageSize,
    totalCount,
    totalPages,
    onPageChange,
}: PaginationProps) {
    const first = (page - 1) * pageSize + 1;
    const last = Math.min(page * pageSize, totalCount);

    return (
        <div className="flex flex-wrap items-center justify-between gap-3 border-t border-border-subtle px-6 py-4 text-sm text-ink-muted">
            <span>
                {first} - {last} of {totalCount}
            </span>
            <div className="flex items-center gap-3">
                <Button
                    variant="secondary"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => onPageChange(page - 1)}
                >
                    Prev
                </Button>
                <span className="text-ink">
                    {page} / {Math.max(totalPages, 1)}
                </span>
                <Button
                    variant="secondary"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => onPageChange(page + 1)}
                >
                    Next
                </Button>
            </div>
        </div>
    );
}

export default Pagination;
