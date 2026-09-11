import { endpoints } from "@/api/api-constants";
import { usePost } from "@/api/api-hooks";
import Banner from "@/components/common/banner";
import { Button } from "@/components/controls/button";
import Dialog from "@/components/controls/dialog";
import { stringTemplateFormat } from "@/utils/format";

const kinds = {
    station: {
        title: "Decommission station",
        endpoint: endpoints.decommissionStation,
        action: "Decommission",
        busy: "Decommissioning…",
        consequence:
            "It stays on the defects already raised against it, but no new defect can use it.",
    },
    errorCode: {
        title: "Retire error code",
        endpoint: endpoints.retireErrorCode,
        action: "Retire",
        busy: "Retiring…",
        consequence:
            "It stays on the defects and escapes it already classifies, but it can't classify new ones.",
    },
    customer: {
        title: "Deactivate customer",
        endpoint: endpoints.deactivateCustomer,
        action: "Deactivate",
        busy: "Deactivating…",
        consequence:
            "It stays on the escapes already reported for it, but no new escape can use it.",
    },
};

export interface LifecycleDialogProps {
    kind: keyof typeof kinds;
    id: string;
    code: string;
    /** The station or customer name, or the error code's description. */
    name: string;
    onClose: () => void;
    /** Called after the change, or when a stale row needs reloading. */
    onDone: () => void;
}

function LifecycleDialog({
    kind,
    id,
    code,
    name,
    onClose,
    onDone,
}: LifecycleDialogProps) {
    const { title, endpoint, action, busy, consequence } = kinds[kind];

    const { post, loading, error } = usePost(
        stringTemplateFormat(endpoint, id),
    );

    const stale =
        error !== null && (error.status === 409 || error.status === 404);

    async function submit() {
        const response = await post();

        if (response) {
            onDone();
        }
    }

    return (
        <Dialog
            title={title}
            onClose={onClose}
            footer={
                <>
                    <Button variant="link" onClick={onClose}>
                        Cancel
                    </Button>
                    <Button
                        disabled={loading || stale}
                        onClick={() => void submit()}
                    >
                        {loading ? busy : action}
                    </Button>
                </>
            }
        >
            <p className="text-[15px] text-ink">
                <span className="font-mono font-bold">{code}</span>{" "}
                <span className="text-ink-muted">{name}</span>
            </p>
            <p className="mt-4 text-sm text-ink-muted">
                {consequence} This can't be undone.
            </p>

            {error ? (
                <div className="mt-5">
                    <Banner
                        message={error.problem?.detail ?? error.message}
                        action={
                            stale ? (
                                <Button
                                    variant="secondary"
                                    size="sm"
                                    onClick={onDone}
                                >
                                    Reload
                                </Button>
                            ) : undefined
                        }
                    />
                </div>
            ) : null}
        </Dialog>
    );
}

export default LifecycleDialog;
