import { useEffect, useRef, type ReactNode } from "react";
import Icon from "@/components/common/icon";
import { Button } from "@/components/controls/button";

export interface DialogProps {
    title: string;
    onClose: () => void;
    children: ReactNode;
    footer?: ReactNode;
}

/** Mount it to open it, unmount it to close it. */
function Dialog({ title, onClose, children, footer }: DialogProps) {
    const dialogRef = useRef<HTMLDialogElement>(null);

    useEffect(() => {
        dialogRef.current?.showModal();
    }, []);

    return (
        <dialog
            ref={dialogRef}
            onCancel={(event) => {
                event.preventDefault();
                onClose();
            }}
            onClick={(event) => {
                if (event.target === dialogRef.current) {
                    onClose();
                }
            }}
            className="m-auto w-117.5 max-w-[calc(100vw-2rem)] rounded-[7px] border border-border-control bg-surface p-0 text-ink backdrop:bg-[#171917]/45"
        >
            <header className="flex items-center justify-between border-b border-border-subtle px-8 pt-7 pb-5">
                <h2 className="font-display text-[26px] font-bold">{title}</h2>
                <Button
                    variant="ghost"
                    size="icon"
                    aria-label="Close"
                    onClick={onClose}
                >
                    <Icon name="close" className="size-4" />
                </Button>
            </header>

            <div className="px-8 py-6">{children}</div>

            {footer ? (
                <footer className="flex items-center justify-end gap-4 border-t border-border-subtle px-8 py-5">
                    {footer}
                </footer>
            ) : null}
        </dialog>
    );
}

export default Dialog;
