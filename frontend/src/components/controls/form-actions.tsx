import type { ReactNode } from "react";

export interface FormActionsProps {
    children: ReactNode;
}

/** Page actions: pinned to the bottom of a phone screen, inline above `md`. */
function FormActions({ children }: FormActionsProps) {
    return (
        <div className="fixed inset-x-0 bottom-0 z-20 flex items-center justify-end gap-4 border-t border-border-subtle bg-surface px-6 py-4 pb-[calc(env(safe-area-inset-bottom)+1rem)] md:static md:mt-8 md:border-border-strong md:bg-transparent md:px-0 md:pt-6 md:pb-0">
            {children}
        </div>
    );
}

export default FormActions;
